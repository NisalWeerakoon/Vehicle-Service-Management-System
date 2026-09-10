using System.ComponentModel.DataAnnotations;
using CustomerBookingService.Models;

namespace CustomerBookingService.DTOs;

public class AdminUserResponseDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int? CustomerId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AdminCreateUserDto
{
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "ServiceAdvisor";

    public string? FullName { get; set; }

    public string? Phone { get; set; }
}

public class UpdateUserRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class ToggleUserStatusDto
{
    public bool IsActive { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }

    public Dictionary<string, int> UsersByRole { get; set; } = new();

    public int TotalBookings { get; set; }

    public Dictionary<string, int> BookingsByStatus { get; set; } = new();

    public int TotalVehicles { get; set; }

    public int TotalCheckIns { get; set; }
}
