using System.Security.Claims;
using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using CustomerBookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly CustomerBookingDbContext _dbContext;

    private readonly IBookingEventPublisher _eventPublisher;

    public BookingsController(CustomerBookingDbContext dbContext, IBookingEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
    }

    // ======================================================
    // CUSTOMER: CREATE BOOKING FOR OWN VEHICLE
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPost("me")]
    public async Task<ActionResult<BookingResponseDto>> CreateMyBooking(
        BookingCreateDto request)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Authenticated user was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        if (request.PreferredDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new
            {
                message = "Preferred service date cannot be in the past."
            });
        }

        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v =>
                v.Id == request.VehicleId &&
                v.CustomerId == user.CustomerId.Value
            );

        if (vehicle is null)
        {
            return BadRequest(new
            {
                message =
                    "The selected vehicle does not belong to your account."
            });
        }

        var booking = new Booking
        {
            BookingReference = GenerateBookingReference(),

            CustomerId = user.CustomerId.Value,

            VehicleId = vehicle.Id,

            PreferredDate = request.PreferredDate,

            RequestedServiceOrProblem =
                request.RequestedServiceOrProblem.Trim(),

            Status = BookingStatus.Pending,

            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Bookings.Add(booking);

        await _dbContext.SaveChangesAsync();

        booking.Vehicle = vehicle;

        await _eventPublisher.PublishBookingCreatedAsync(
            booking,
            HttpContext.RequestAborted
        );

        return Created(
            $"/api/bookings/me/{booking.Id}",
            ToResponse(booking)
        );
    }

    // ======================================================
    // CUSTOMER: VIEW OWN BOOKINGS
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>>
        GetMyBookings()
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Authenticated user was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        var bookings = await _dbContext.Bookings
            .Include(b => b.Vehicle)
            .Where(b => b.CustomerId == user.CustomerId.Value)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(
            bookings.Select(ToResponse)
        );
    }

    // ======================================================
    // CUSTOMER: VIEW ONE OWN BOOKING
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpGet("me/{id:int}")]
    public async Task<ActionResult<BookingResponseDto>>
        GetMyBookingById(int id)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Authenticated user was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        var booking = await _dbContext.Bookings
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b =>
                b.Id == id &&
                b.CustomerId == user.CustomerId.Value
            );

        if (booking is null)
        {
            return NotFound(new
            {
                message = "Booking was not found."
            });
        }

        return Ok(ToResponse(booking));
    }

    // ======================================================
    // CUSTOMER: UPDATE OWN ELIGIBLE BOOKING
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPut("me/{id:int}")]
    public async Task<ActionResult<BookingResponseDto>>
        UpdateMyBooking(
            int id,
            BookingUpdateDto request)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
        {
            return Unauthorized(new
            {
                message =
                    "Authenticated user was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Customer profile must be created first."
            });
        }

        var booking =
            await _dbContext.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.CustomerId ==
                        user.CustomerId.Value
                );

        if (booking is null)
        {
            return NotFound(new
            {
                message = "Booking was not found."
            });
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            return BadRequest(new
            {
                message =
                    "This booking can no longer be updated because vehicle check-in has started."
            });
        }

        if (request.PreferredDate.Date <
            DateTime.UtcNow.Date)
        {
            return BadRequest(new
            {
                message =
                    "Preferred service date cannot be in the past."
            });
        }

        booking.PreferredDate =
            request.PreferredDate;

        booking.RequestedServiceOrProblem =
            request.RequestedServiceOrProblem.Trim();

        booking.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(booking));
    }

    // ======================================================
    // CUSTOMER: CANCEL OWN ELIGIBLE BOOKING
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPatch("me/{id:int}/cancel")]
    public async Task<ActionResult<BookingResponseDto>>
        CancelMyBooking(int id)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
        {
            return Unauthorized(new
            {
                message =
                    "Authenticated user was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Customer profile must be created first."
            });
        }

        var booking =
            await _dbContext.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.CustomerId ==
                        user.CustomerId.Value
                );

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
                message =
                    "This booking is already cancelled."
            });
        }

        if (booking.Status == BookingStatus.InService ||
            booking.Status == BookingStatus.Completed)
        {
            return BadRequest(new
            {
                message =
                    "This booking cannot be cancelled because service has already begun."
            });
        }

        booking.Status =
            BookingStatus.Cancelled;

        booking.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(booking));
    }

    // ======================================================
    // STAFF: CREATE BOOKING FOR CUSTOMER
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpPost("staff")]
    public async Task<ActionResult<BookingResponseDto>>
        CreateBookingByStaff(
            StaffBookingCreateDto request)
    {
        if (request.PreferredDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new
            {
                message = "Preferred service date cannot be in the past."
            });
        }

        var customer =
            await _dbContext.Customers
                .FirstOrDefaultAsync(c =>
                    c.Id == request.CustomerId
                );

        if (customer is null)
        {
            return NotFound(new
            {
                message = "Customer was not found."
            });
        }

        var vehicle =
            await _dbContext.Vehicles
                .FirstOrDefaultAsync(v =>
                    v.Id == request.VehicleId &&
                    v.CustomerId == request.CustomerId
                );

        if (vehicle is null)
        {
            return BadRequest(new
            {
                message =
                    "The selected vehicle does not belong to this customer."
            });
        }

        var booking = new Booking
        {
            BookingReference =
                GenerateBookingReference(),

            CustomerId =
                request.CustomerId,

            VehicleId =
                request.VehicleId,

            PreferredDate =
                request.PreferredDate,

            RequestedServiceOrProblem =
                request.RequestedServiceOrProblem.Trim(),

            Status =
                BookingStatus.Pending,

            CreatedAt =
                DateTime.UtcNow
        };

        _dbContext.Bookings.Add(booking);

        await _dbContext.SaveChangesAsync();

        booking.Vehicle = vehicle;

        await _eventPublisher.PublishBookingCreatedAsync(
            booking,
            HttpContext.RequestAborted
        );

        return Created(
            $"/api/bookings/{booking.Id}",
            ToResponse(booking)
        );
    }

    // ======================================================
    // STAFF: VIEW BOOKING
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponseDto>>
        GetBookingById(int id)
    {
        var booking =
            await _dbContext.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
        {
            return NotFound(new
            {
                message = "Booking was not found."
            });
        }

        return Ok(ToResponse(booking));
    }

    // ======================================================
    // STAFF: UPDATE ELIGIBLE BOOKING
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookingResponseDto>>
        UpdateBookingByStaff(
            int id,
            BookingUpdateDto request)
    {
        var booking =
            await _dbContext.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b =>
                    b.Id == id
                );

        if (booking is null)
        {
            return NotFound(new
            {
                message = "Booking was not found."
            });
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            return BadRequest(new
            {
                message =
                    "Booking cannot be updated after vehicle check-in."
            });
        }

        if (request.PreferredDate.Date <
            DateTime.UtcNow.Date)
        {
            return BadRequest(new
            {
                message =
                    "Preferred service date cannot be in the past."
            });
        }

        booking.PreferredDate =
            request.PreferredDate;

        booking.RequestedServiceOrProblem =
            request.RequestedServiceOrProblem.Trim();

        booking.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(booking));
    }

    // ======================================================
    // STAFF: CANCEL ELIGIBLE BOOKING
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponseDto>>
        CancelBookingByStaff(int id)
    {
        var booking =
            await _dbContext.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b =>
                    b.Id == id
                );

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
                message =
                    "This booking is already cancelled."
            });
        }

        if (booking.Status == BookingStatus.InService ||
            booking.Status == BookingStatus.Completed)
        {
            return BadRequest(new
            {
                message =
                    "Booking cannot be cancelled because service has already begun."
            });
        }

        booking.Status =
            BookingStatus.Cancelled;

        booking.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(booking));
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private async Task<User?> GetCurrentUserAsync()
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            return null;
        }

        return await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userId
            );
    }

    private static string GenerateBookingReference()
    {
        return
            $"BKG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    private static BookingResponseDto ToResponse(
        Booking booking)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,

            BookingReference =
                booking.BookingReference,

            CustomerId =
                booking.CustomerId,

            VehicleId =
                booking.VehicleId,

            VehicleRegistrationNumber =
                booking.Vehicle?.RegistrationNumber
                ?? string.Empty,

            VehicleName =
                booking.Vehicle is null
                    ? string.Empty
                    : $"{booking.Vehicle.Make} {booking.Vehicle.Model}",

            PreferredDate =
                booking.PreferredDate,

            RequestedServiceOrProblem =
                booking.RequestedServiceOrProblem,

            Status =
                booking.Status.ToString(),

            CreatedAt =
                booking.CreatedAt,

            UpdatedAt =
                booking.UpdatedAt
        };
    }
}