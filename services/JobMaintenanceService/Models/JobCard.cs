using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class JobCard
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string JobCardNumber { get; set; } = string.Empty;

    [Required]
    public int CheckInId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(30)]
    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ReportedProblems { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Created";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
