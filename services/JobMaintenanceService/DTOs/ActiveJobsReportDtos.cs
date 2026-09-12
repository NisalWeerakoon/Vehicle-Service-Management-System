namespace JobMaintenanceService.DTOs;

public class ActiveJobReportItemDto
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
    public string? AssignedMechanicId { get; set; }
    public string? AssignedMechanicName { get; set; }
}

public class ActiveJobStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ActiveJobsReportDto
{
    public DateTime GeneratedAt { get; set; }
    public int TotalActiveJobs { get; set; }
    public string? AppliedStatusFilter { get; set; }
    public List<ActiveJobStatusCountDto> StatusCounts { get; set; } = new();
    public List<ActiveJobReportItemDto> Jobs { get; set; } = new();
}
