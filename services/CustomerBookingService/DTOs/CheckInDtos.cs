using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.DTOs;

public class BookingCheckInDto
{
    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }

    [Required]
    [MaxLength(500)]
    public string ReportedProblems { get; set; } = string.Empty;
}

public class WalkInCheckInDto
{
    // Customer information
    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Address { get; set; }

    // Vehicle information
    [Required]
    [MaxLength(30)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    [MaxLength(40)]
    public string FuelType { get; set; } = string.Empty;

    // Check-in information
    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }

    [Required]
    [MaxLength(500)]
    public string ReportedProblems { get; set; } = string.Empty;
}

public class CheckInResponseDto
{
    public int Id { get; set; }

    public int? BookingId { get; set; }

    public string? BookingReference { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    public string VehicleName { get; set; } = string.Empty;

    public DateTime CheckInDateTime { get; set; }

    public int Mileage { get; set; }

    public string ReportedProblems { get; set; } = string.Empty;

    public bool IsWalkIn { get; set; }

    public string ServiceStatus { get; set; } = string.Empty;
}
