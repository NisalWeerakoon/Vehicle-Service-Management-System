using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class ServiceHistory
{
    public int Id { get; set; }
    public int JobCardId { get; set; }

    [Required, MaxLength(30)]
    public string JobCardNumber { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    [Required, MaxLength(30)]
    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    [Required, MaxLength(500)]
    public string ReportedProblems { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string CompletedBy { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string FinalStatus { get; set; } = string.Empty;

    public DateTime CompletedAt { get; set; }
    public DateTime CapturedAt { get; set; }
    public string CompletedRepairTasksJson { get; set; } = "[]";
    public string RepairNotesJson { get; set; } = "[]";
    public string? InspectionResults { get; set; }
    public string? IdentifiedProblems { get; set; }
    public DateTime? InspectionCompletedAt { get; set; }
}
