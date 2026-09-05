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
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly CustomerBookingDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthController(
        CustomerBookingDbContext dbContext,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    // --------------------------------------------------
    // REGISTER CUSTOMER ACCOUNT
    // --------------------------------------------------

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterDto request)
    {
        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var existingUser =
            await _dbContext.Users
                .AnyAsync(u => u.Email == email);

        if (existingUser)
        {
            return BadRequest(new
            {
                message =
                    "An account with this email already exists."
            });
        }

        var user = new User
        {
            Email = email,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password
                ),

            Role = UserRole.Customer,

            IsActive = true
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        var token =
            _tokenService.CreateToken(user);

        return Ok(
            new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt =
                    DateTime.UtcNow.AddHours(8)
            }
        );
    }

    // --------------------------------------------------
    // LOGIN
    // --------------------------------------------------

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginDto request)
    {
        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var user =
            await _dbContext.Users
                .FirstOrDefaultAsync(
                    u => u.Email == email
                );

        if (user is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid email or password."
            });
        }

        var validPassword =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

        if (!validPassword)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid email or password."
            });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message =
                    "This account has been deactivated."
            });
        }

        var token =
            _tokenService.CreateToken(user);

        return Ok(
            new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt =
                    DateTime.UtcNow.AddHours(8)
            }
        );
    }

    // --------------------------------------------------
    // LOGOUT
    // --------------------------------------------------

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        /*
         * JWT authentication is stateless.
         *
         * React will remove the stored JWT when
         * the user logs out.
         */

        return Ok(new
        {
            message =
                "Logout successful."
        });
    }

    // --------------------------------------------------
    // CURRENT LOGGED-IN USER
    // --------------------------------------------------

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var email =
            User.FindFirstValue(
                ClaimTypes.Email
            );

        var role =
            User.FindFirstValue(
                ClaimTypes.Role
            );

        return Ok(new
        {
            userId,
            email,
            role
        });
    }

    // --------------------------------------------------
    // ROLE AUTHORIZATION TEST
    // --------------------------------------------------

    [Authorize(Roles = "Administrator")]
    [HttpGet("admin-test")]
    public IActionResult AdminTest()
    {
        return Ok(new
        {
            message =
                "Administrator authorization works."
        });
    }

    // --------------------------------------------------
    // ACTIVE MECHANICS LIST
    // --------------------------------------------------

    [Authorize]
    [HttpGet("mechanics")]
    public async Task<IActionResult> GetActiveMechanics()
    {
        var mechanics = await _dbContext.Users
            .Where(u => u.Role == UserRole.Mechanic && u.IsActive)
            .Select(u => new
            {
                u.Id,
                u.Email
            })
            .ToListAsync();

        return Ok(mechanics);
    }
}