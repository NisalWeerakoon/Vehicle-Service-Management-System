using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class UpdateJobStatusDto
{
    [Required, MaxLength(30)]
    public string Status { get; set; } = string.Empty;
}

public class JobStatusHistoryDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string ChangedByRole { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

public class JobStatusResponseDto
{
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public List<string> AllowedNextStatuses { get; set; } = new();
}
