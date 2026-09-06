using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class CreateInspectionDto
{
    [Range(1, int.MaxValue)]
    public int JobCardId { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    public string InspectionResults { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    public string IdentifiedProblems { get; set; } = string.Empty;
}

public class InspectionResponseDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string MechanicId { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
    public string InspectionResults { get; set; } = string.Empty;
    public string IdentifiedProblems { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletionEventId { get; set; }
}

public class InspectionCompletedEventData
{
    public int InspectionId { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public string MechanicId { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
    public string InspectionResults { get; set; } = string.Empty;
    public string IdentifiedProblems { get; set; } = string.Empty;
}

public class InspectionCompletedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = "InspectionCompleted";
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = "JobMaintenanceService";
    public string? CorrelationId { get; set; }
    public InspectionCompletedEventData Data { get; set; } = new();
}
