using CustomerBookingService.Data;
using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    private readonly CustomerBookingDbContext _dbContext;

    public AdminController(CustomerBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ======================================================
    // GET ALL USERS
    // ======================================================

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserResponseDto>>> GetAllUsers()
    {
        var users = await _dbContext.Users
            .Include(u => u.Customer)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CustomerId = u.CustomerId,
                FullName = u.Customer != null ? u.Customer.FullName : null,
                Phone = u.Customer != null ? u.Customer.Phone : null,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // ======================================================
    // CREATE USER / STAFF ACCOUNT
    // ======================================================

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserResponseDto>> CreateUser(
        AdminCreateUserDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Email == email);

        if (existingUser)
        {
            return BadRequest(new
            {
                message = "An account with this email already exists."
            });
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            return BadRequest(new
            {
                message = $"Invalid role specified: '{request.Role}'."
            });
        }

        Customer? customer = null;
        if (role == UserRole.Customer && !string.IsNullOrWhiteSpace(request.FullName))
        {
            var customerEmailExists = await _dbContext.Customers
                .AnyAsync(c => c.Email == email);

            if (customerEmailExists)
            {
                return BadRequest(new
                {
                    message = "A customer profile with this email already exists."
                });
            }

            customer = new Customer
            {
                FullName = request.FullName.Trim(),
                Email = email,
                Phone = request.Phone?.Trim() ?? string.Empty
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();
        }

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            IsActive = true,
            CustomerId = customer?.Id,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var response = new AdminUserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CustomerId = user.CustomerId,
            FullName = customer?.FullName,
            Phone = customer?.Phone,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetAllUsers),
            new { id = user.Id },
            response
        );
    }

    // ======================================================
    // UPDATE USER ROLE
    // ======================================================

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        UpdateUserRoleDto request)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            return BadRequest(new { message = $"Invalid role: '{request.Role}'." });
        }

        user.Role = parsedRole;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = $"User #{id} role updated to {user.Role}.",
            userId = user.Id,
            role = user.Role.ToString()
        });
    }

    // ======================================================
    // TOGGLE USER ACTIVE STATUS
    // ======================================================

    [HttpPatch("users/{id:int}/status")]
    public async Task<IActionResult> ToggleUserStatus(
        int id,
        ToggleUserStatusDto request)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.IsActive = request.IsActive;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = $"User #{id} status set to {(user.IsActive ? "Active" : "Inactive")}.",
            userId = user.Id,
            isActive = user.IsActive
        });
    }

    // ======================================================
    // GET DASHBOARD STATS
    // ======================================================

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetDashboardStats()
    {
        var totalUsers = await _dbContext.Users.CountAsync();

        var usersByRoleRaw = await _dbContext.Users
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var usersByRole = usersByRoleRaw.ToDictionary(x => x.Role, x => x.Count);

        var totalBookings = await _dbContext.Bookings.CountAsync();

        var bookingsByStatusRaw = await _dbContext.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var bookingsByStatus = bookingsByStatusRaw.ToDictionary(x => x.Status, x => x.Count);

        var totalVehicles = await _dbContext.Vehicles.CountAsync();
        var totalCheckIns = await _dbContext.CheckIns.CountAsync();

        return Ok(new AdminStatsDto
        {
            TotalUsers = totalUsers,
            UsersByRole = usersByRole,
            TotalBookings = totalBookings,
            BookingsByStatus = bookingsByStatus,
            TotalVehicles = totalVehicles,
            TotalCheckIns = totalCheckIns
        });
    }
}
