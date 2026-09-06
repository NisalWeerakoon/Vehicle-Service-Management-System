using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class MechanicAssignment
{
    public int Id { get; set; }

    [Required]
    public int JobCardId { get; set; }

    [Required]
    [MaxLength(100)]
    public string MechanicId { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string MechanicName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
