using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class CreateRepairTaskDto
{
    [Range(1, int.MaxValue)]
    public int JobCardId { get; set; }

    [Required, MinLength(3), MaxLength(200)]
    public string TaskTitle { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(1000)]
    public string TaskDescription { get; set; } = string.Empty;
}

public class UpdateRepairTaskDto
{
    [Required, MinLength(3), MaxLength(200)]
    public string TaskTitle { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(1000)]
    public string TaskDescription { get; set; } = string.Empty;
}

public class RepairTaskResponseDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string MechanicId { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
