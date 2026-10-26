namespace JobMaintenanceService.DTOs;

public class VehicleServiceHistoryResponseDto
{
    public int VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public List<ServiceHistoryResponseDto> Records { get; set; } = [];
}

public class ServiceHistoryResponseDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string FinalStatus { get; set; } = string.Empty;
    public string ReportedProblems { get; set; } = string.Empty;
    public List<CompletedRepairTaskHistoryDto> CompletedRepairTasks { get; set; } = [];
    public List<RepairNoteHistoryDto> RepairNotes { get; set; } = [];
    public InspectionHistoryDto? Inspection { get; set; }
}

public class CompletedRepairTaskHistoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}

public class RepairNoteHistoryDto
{
    public int Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InspectionHistoryDto
{
    public string? InspectionResults { get; set; }
    public string? IdentifiedProblems { get; set; }
    public DateTime? CompletedAt { get; set; }
}
