using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.Models;

public class Inspection
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
    [MinLength(3)]
    [MaxLength(2000)]
    public string InspectionResults { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    public string IdentifiedProblems { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletionEventId { get; set; }
}
