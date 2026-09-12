using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class JobStatusHistory
{
    public int Id { get; set; }

    [Required]
    public int JobCardId { get; set; }

    [Required, MaxLength(30)]
    public string FromStatus { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string ToStatus { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string ChangedByRole { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
