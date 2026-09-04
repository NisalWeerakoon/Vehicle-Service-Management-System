using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using CustomerBookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Controllers;

[ApiController]
[Route("api/check-ins")]
[Authorize(Roles = "ServiceAdvisor,Administrator")]
public class CheckInsController : ControllerBase
{
    private readonly CustomerBookingDbContext _dbContext;
    private readonly ICheckInEventPublisher _eventPublisher;

    public CheckInsController(
        CustomerBookingDbContext dbContext,
        ICheckInEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
    }

    // ======================================================
    // STAFF: CHECK IN EXISTING BOOKING
    // ======================================================

    [HttpPost("booking/{bookingId:int}")]
    public async Task<ActionResult<CheckInResponseDto>> CheckInBooking(
        int bookingId,
        BookingCheckInDto request)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            return NotFound(new
            {
                message = "Booking was not found."
            });
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return BadRequest(new
            {
                message = "Cancelled bookings cannot be checked in."
            });
        }

        if (booking.Status == BookingStatus.CheckedIn ||
            booking.Status == BookingStatus.InService ||
            booking.Status == BookingStatus.Completed)
        {
            return Conflict(new
            {
                message = "This booking has already been checked in or service has already started."
            });
        }

        var duplicate = await _dbContext.CheckIns
            .AnyAsync(c =>
                c.BookingId == booking.Id &&
                c.IsActive);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "This booking already has an active check-in."
            });
        }

        if (booking.Vehicle is null)
        {
            return BadRequest(new
            {
                message = "The booking vehicle could not be found."
            });
        }

        var activeVehicleCheckIn = await _dbContext.CheckIns
            .AnyAsync(c =>
                c.VehicleId == booking.VehicleId &&
                c.IsActive);

        if (activeVehicleCheckIn)
        {
            return Conflict(new
            {
                message = "This vehicle already has an active check-in."
            });
        }

        var previousMileage = await _dbContext.CheckIns
            .Where(c => c.VehicleId == booking.VehicleId)
            .OrderByDescending(c => c.CheckInDateTime)
            .Select(c => (int?)c.Mileage)
            .FirstOrDefaultAsync();

        if (previousMileage.HasValue &&
            request.Mileage < previousMileage.Value)
        {
            return BadRequest(new
            {
                message =
                    $"Mileage cannot be lower than the previous recorded mileage ({previousMileage.Value})."
            });
        }

        var checkIn = new CheckIn
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            VehicleId = booking.VehicleId,
            CheckInDateTime = DateTime.UtcNow,
            Mileage = request.Mileage,
            ReportedProblems = request.ReportedProblems.Trim(),
            IsActive = true,
            IsWalkIn = false,
            CreatedAt = DateTime.UtcNow
        };

        // FR-CHK-06: update service/booking status.
        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedAt = DateTime.UtcNow;

        _dbContext.CheckIns.Add(checkIn);
        await _dbContext.SaveChangesAsync();

        checkIn.Booking = booking;
        checkIn.Vehicle = booking.Vehicle;

        await _eventPublisher.PublishVehicleCheckedInAsync(
            checkIn,
            HttpContext.RequestAborted);

        return Created(
            $"/api/check-ins/{checkIn.Id}",
            ToResponse(checkIn));
    }

    // ======================================================
    // STAFF: WALK-IN CHECK IN
    // ======================================================

    [HttpPost("walk-in")]
    public async Task<ActionResult<CheckInResponseDto>> CheckInWalkIn(
        WalkInCheckInDto request)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var email = request.Email.Trim().ToLowerInvariant();
        var registrationNumber =
            request.RegistrationNumber.Trim().ToUpperInvariant();

        if (request.Year < 1900 ||
            request.Year > DateTime.UtcNow.Year + 1)
        {
            return BadRequest(new
            {
                message = "Vehicle year is invalid."
            });
        }

        // Reuse an existing customer with the same email.
        // Otherwise create a new walk-in customer.
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == email);

        if (customer is null)
        {
            customer = new Customer
            {
                FullName = request.FullName.Trim(),
                Email = email,
                Phone = request.Phone.Trim(),
                Address = request.Address?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();
        }

        // Reuse the vehicle if it belongs to this customer.
        // A registration number belonging to another customer is invalid.
        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v =>
                v.RegistrationNumber == registrationNumber);

        if (vehicle is not null &&
            vehicle.CustomerId != customer.Id)
        {
            return Conflict(new
            {
                message =
                    "This registration number is already registered to another customer."
            });
        }

        if (vehicle is null)
        {
            vehicle = new Vehicle
            {
                CustomerId = customer.Id,
                RegistrationNumber = registrationNumber,
                Make = request.Make.Trim(),
                Model = request.Model.Trim(),
                Year = request.Year,
                FuelType = request.FuelType.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();
        }

        var activeVehicleCheckIn = await _dbContext.CheckIns
            .AnyAsync(c =>
                c.VehicleId == vehicle.Id &&
                c.IsActive);

        if (activeVehicleCheckIn)
        {
            return Conflict(new
            {
                message = "This vehicle already has an active check-in."
            });
        }

        var previousMileage = await _dbContext.CheckIns
            .Where(c => c.VehicleId == vehicle.Id)
            .OrderByDescending(c => c.CheckInDateTime)
            .Select(c => (int?)c.Mileage)
            .FirstOrDefaultAsync();

        if (previousMileage.HasValue &&
            request.Mileage < previousMileage.Value)
        {
            return BadRequest(new
            {
                message =
                    $"Mileage cannot be lower than the previous recorded mileage ({previousMileage.Value})."
            });
        }

        // A walk-in is represented as a booking with the current date,
        // so the existing service/booking workflow can continue normally.
        var booking = new Booking
        {
            BookingReference = GenerateBookingReference(),
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PreferredDate = DateTime.UtcNow,
            RequestedServiceOrProblem =
                request.ReportedProblems.Trim(),
            Status = BookingStatus.CheckedIn,
            CreatedAt = DateTime.UtcNow
        };

        var checkIn = new CheckIn
        {
            Booking = booking,
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            CheckInDateTime = DateTime.UtcNow,
            Mileage = request.Mileage,
            ReportedProblems = request.ReportedProblems.Trim(),
            IsActive = true,
            IsWalkIn = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Bookings.Add(booking);
        _dbContext.CheckIns.Add(checkIn);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        checkIn.Booking = booking;
        checkIn.Vehicle = vehicle;

        await _eventPublisher.PublishVehicleCheckedInAsync(
            checkIn,
            HttpContext.RequestAborted);

        return Created(
            $"/api/check-ins/{checkIn.Id}",
            ToResponse(checkIn));
    }

    private static string GenerateBookingReference()
    {
        return
            $"BKG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    private static CheckInResponseDto ToResponse(CheckIn checkIn)
    {
        return new CheckInResponseDto
        {
            Id = checkIn.Id,
            BookingId = checkIn.BookingId,
            BookingReference = checkIn.Booking?.BookingReference,
            CustomerId = checkIn.CustomerId,
            CustomerName = checkIn.Customer?.FullName ?? string.Empty,
            VehicleId = checkIn.VehicleId,
            VehicleRegistrationNumber =
                checkIn.Vehicle?.RegistrationNumber ?? string.Empty,
            VehicleName = checkIn.Vehicle is null
                ? string.Empty
                : $"{checkIn.Vehicle.Make} {checkIn.Vehicle.Model}",
            CheckInDateTime = checkIn.CheckInDateTime,
            Mileage = checkIn.Mileage,
            ReportedProblems = checkIn.ReportedProblems,
            IsWalkIn = checkIn.IsWalkIn,
            ServiceStatus =
                checkIn.Booking?.Status.ToString() ?? "CheckedIn"
        };
    }
}
