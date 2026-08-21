using System.Security.Claims;
using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly CustomerBookingDbContext _dbContext;

    public CustomersController(CustomerBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ======================================================
    // GET MY CUSTOMER PROFILE
    // Customer only
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpGet("me")]
    public async Task<ActionResult<CustomerResponseDto>> GetMyProfile()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authenticated user."
            });
        }

        var user = await _dbContext.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "User account was not found."
            });
        }

        if (user.Customer is null)
        {
            return NotFound(new
            {
                message = "Customer profile has not been created."
            });
        }

        return Ok(ToResponse(user.Customer));
    }

    // ======================================================
    // CREATE MY CUSTOMER PROFILE
    // Customer only
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPost("me")]
    public async Task<ActionResult<CustomerResponseDto>> CreateMyProfile(
        CustomerCreateDto request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

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

        if (user.CustomerId.HasValue)
        {
            return BadRequest(new
            {
                message = "Customer profile already exists."
            });
        }

        var email =
            request.Email.Trim().ToLowerInvariant();

        // A Customer profile must use the same email
        // as the authenticated login account.
        if (!string.Equals(
                email,
                user.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "Customer profile email must match the logged-in account email."
            });
        }

        var emailExists = await _dbContext.Customers
            .AnyAsync(c => c.Email == email);

        if (emailExists)
        {
            return BadRequest(new
            {
                message =
                    "A customer profile with this email already exists."
            });
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Address = request.Address?.Trim()
        };

        _dbContext.Customers.Add(customer);

        await _dbContext.SaveChangesAsync();

        user.CustomerId = customer.Id;

        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMyProfile),
            null,
            ToResponse(customer)
        );
    }

    // ======================================================
    // UPDATE MY PROFILE
    // Customer only
    // ======================================================

    [Authorize(Roles = "Customer")]
    [HttpPut("me")]
    public async Task<ActionResult<CustomerResponseDto>> UpdateMyProfile(
        CustomerUpdateDto request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid authenticated user."
            });
        }

        var user = await _dbContext.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "User account was not found."
            });
        }

        if (user.Customer is null)
        {
            return NotFound(new
            {
                message = "Customer profile was not found."
            });
        }

        user.Customer.FullName =
            request.FullName.Trim();

        user.Customer.Phone =
            request.Phone.Trim();

        user.Customer.Address =
            request.Address?.Trim();

        user.Customer.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(
            ToResponse(user.Customer)
        );
    }

    // ======================================================
    // STAFF: REGISTER CUSTOMER
    // ======================================================

    [Authorize(
        Roles =
            "ServiceAdvisor,Administrator"
    )]
    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> RegisterCustomerByStaff(
        CustomerCreateDto request)
    {
        var email =
            request.Email.Trim().ToLowerInvariant();

        var exists =
            await _dbContext.Customers
                .AnyAsync(c => c.Email == email);

        if (exists)
        {
            return BadRequest(new
            {
                message =
                    "A customer with this email already exists."
            });
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Address = request.Address?.Trim()
        };

        _dbContext.Customers.Add(customer);

        await _dbContext.SaveChangesAsync();

        return Created(
            $"/api/customers/{customer.Id}",
            ToResponse(customer)
        );
    }

    // ======================================================
    // STAFF: GET CUSTOMER BY ID
    // ======================================================

    [Authorize(
        Roles =
            "ServiceAdvisor,Administrator"
    )]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponseDto>> GetCustomerById(
        int id)
    {
        var customer =
            await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return NotFound(new
            {
                message = "Customer was not found."
            });
        }

        return Ok(ToResponse(customer));
    }

    // ======================================================
    // DTO MAPPING
    // ======================================================

    private static CustomerResponseDto ToResponse(
        Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}