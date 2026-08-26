using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.DTOs;

public class BookingCreateDto
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public DateTime PreferredDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string RequestedServiceOrProblem { get; set; } = string.Empty;
}

public class StaffBookingCreateDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public DateTime PreferredDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string RequestedServiceOrProblem { get; set; } = string.Empty;
}

public class BookingResponseDto
{
    public int Id { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public int VehicleId { get; set; }

    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    public string VehicleName { get; set; } = string.Empty;

    public DateTime PreferredDate { get; set; }

    public string RequestedServiceOrProblem { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class BookingUpdateDto
{
    [Required]
    public DateTime PreferredDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string RequestedServiceOrProblem { get; set; } = string.Empty;
}