using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class RepairTask
{
    public int Id { get; set; }

    [Required]
    public int JobCardId { get; set; }

    [Required, MaxLength(100)]
    public string MechanicId { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string MechanicName { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(200)]
    public string TaskTitle { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(1000)]
    public string TaskDescription { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
