using System.Security.Claims;
using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly CustomerBookingDbContext _dbContext;

    public VehiclesController(CustomerBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ======================================================
    // CUSTOMER: GET MY VEHICLES
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<VehicleResponseDto>>> GetMyVehicles()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authenticated user."
            });
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "User account was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        var vehicles = await _dbContext.Vehicles
            .Where(v => v.CustomerId == user.CustomerId.Value)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => ToResponse(v))
            .ToListAsync();

        return Ok(vehicles);
    }

    // ======================================================
    // CUSTOMER: GET ONE OF MY VEHICLES
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpGet("me/{id:int}")]
    public async Task<ActionResult<VehicleResponseDto>> GetMyVehicleById(int id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authenticated user."
            });
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "User account was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.CustomerId == user.CustomerId.Value
            );

        if (vehicle is null)
        {
            return NotFound(new
            {
                message = "Vehicle was not found."
            });
        }

        return Ok(ToResponse(vehicle));
    }

    // ======================================================
    // CUSTOMER: ADD VEHICLE
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPost("me")]
    public async Task<ActionResult<VehicleResponseDto>> CreateMyVehicle(
        VehicleCreateDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authenticated user."
            });
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "User account was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        var registrationNumber =
            NormalizeRegistrationNumber(request.RegistrationNumber);

        var duplicate = await _dbContext.Vehicles
            .AnyAsync(v => v.RegistrationNumber == registrationNumber);

        if (duplicate)
        {
            return BadRequest(new
            {
                message = "A vehicle with this registration number already exists."
            });
        }

        if (!IsValidYear(request.Year))
        {
            return BadRequest(new
            {
                message = "Vehicle year is invalid."
            });
        }

        var vehicle = new Vehicle
        {
            CustomerId = user.CustomerId.Value,
            RegistrationNumber = registrationNumber,
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            FuelType = request.FuelType.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Vehicles.Add(vehicle);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new
            {
                message = "Unable to register this vehicle. Check the vehicle details."
            });
        }

        return Created(
            $"/api/vehicles/me/{vehicle.Id}",
            ToResponse(vehicle)
        );
    }

    // ======================================================
    // CUSTOMER: UPDATE MY VEHICLE
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPut("me/{id:int}")]
    public async Task<ActionResult<VehicleResponseDto>> UpdateMyVehicle(
        int id,
        VehicleUpdateDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authenticated user."
            });
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "User account was not found."
            });
        }

        if (!user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile must be created first."
            });
        }

        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.CustomerId == user.CustomerId.Value
            );

        if (vehicle is null)
        {
            return NotFound(new
            {
                message = "Vehicle was not found."
            });
        }

        if (!IsValidYear(request.Year))
        {
            return BadRequest(new
            {
                message = "Vehicle year is invalid."
            });
        }

        vehicle.Make = request.Make.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.FuelType = request.FuelType.Trim();
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(vehicle));
    }

    // ======================================================
    // STAFF: REGISTER VEHICLE FOR CUSTOMER
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpPost("customer/{customerId:int}")]
    public async Task<ActionResult<VehicleResponseDto>> CreateVehicleForCustomer(
        int customerId,
        VehicleCreateDto request)
    {
        var customerExists =
            await _dbContext.Customers.AnyAsync(c => c.Id == customerId);

        if (!customerExists)
        {
            return NotFound(new
            {
                message = "Customer was not found."
            });
        }

        var registrationNumber =
            NormalizeRegistrationNumber(request.RegistrationNumber);

        var duplicate =
            await _dbContext.Vehicles.AnyAsync(v =>
                v.RegistrationNumber == registrationNumber
            );

        if (duplicate)
        {
            return BadRequest(new
            {
                message = "A vehicle with this registration number already exists."
            });
        }

        if (!IsValidYear(request.Year))
        {
            return BadRequest(new
            {
                message = "Vehicle year is invalid."
            });
        }

        var vehicle = new Vehicle
        {
            CustomerId = customerId,
            RegistrationNumber = registrationNumber,
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            FuelType = request.FuelType.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Vehicles.Add(vehicle);

        await _dbContext.SaveChangesAsync();

        return Created(
            $"/api/vehicles/{vehicle.Id}",
            ToResponse(vehicle)
        );
    }

    // ======================================================
    // STAFF: GET VEHICLES FOR CUSTOMER
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<IEnumerable<VehicleResponseDto>>>
        GetVehiclesForCustomer(int customerId)
    {
        var customerExists =
            await _dbContext.Customers.AnyAsync(c => c.Id == customerId);

        if (!customerExists)
        {
            return NotFound(new
            {
                message = "Customer was not found."
            });
        }

        var vehicles = await _dbContext.Vehicles
            .Where(v => v.CustomerId == customerId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => ToResponse(v))
            .ToListAsync();

        return Ok(vehicles);
    }

    // ======================================================
    // STAFF: UPDATE VEHICLE
    // ======================================================

    [Authorize(Roles = "ServiceAdvisor,Administrator")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleResponseDto>> UpdateVehicleByStaff(
        int id,
        VehicleUpdateDto request)
    {
        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null)
        {
            return NotFound(new
            {
                message = "Vehicle was not found."
            });
        }

        if (!IsValidYear(request.Year))
        {
            return BadRequest(new
            {
                message = "Vehicle year is invalid."
            });
        }

        vehicle.Make = request.Make.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.FuelType = request.FuelType.Trim();
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(vehicle));
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private static string NormalizeRegistrationNumber(string registrationNumber)
    {
        return registrationNumber
            .Trim()
            .ToUpperInvariant();
    }

    private static bool IsValidYear(int year)
    {
        var maximumYear = DateTime.UtcNow.Year + 1;

        return year >= 1900 &&
               year <= maximumYear;
    }

    private static VehicleResponseDto ToResponse(Vehicle vehicle)
    {
        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            FuelType = vehicle.FuelType,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt
        };
    }
}