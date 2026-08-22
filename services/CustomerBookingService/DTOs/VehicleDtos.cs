using System.ComponentModel.DataAnnotations;

namespace CustomerBookingService.DTOs;

public class VehicleCreateDto
{
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
}

public class VehicleUpdateDto
{
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
}

public class VehicleResponseDto
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string FuelType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}