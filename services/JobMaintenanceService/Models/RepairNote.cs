using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class RepairNote
{
    public int Id { get; set; }

    [Required]
    public int JobCardId { get; set; }

    [Required, MaxLength(100)]
    public string MechanicId { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string MechanicName { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(2000)]
    public string Note { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
