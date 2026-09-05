using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class CreateJobCardDto
{
    [Range(1, int.MaxValue)]
    public int CheckInId { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(30)]
    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ReportedProblems { get; set; } = string.Empty;
}

public class JobCardResponseDto
{
    public int Id { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int CheckInId { get; set; }
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string ReportedProblems { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
