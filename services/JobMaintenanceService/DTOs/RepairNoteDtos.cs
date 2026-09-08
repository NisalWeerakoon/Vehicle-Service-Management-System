using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class CreateRepairNoteDto
{
    [Range(1, int.MaxValue)]
    public int JobCardId { get; set; }

    [Required, MinLength(3), MaxLength(2000)]
    public string Note { get; set; } = string.Empty;
}

public class UpdateRepairNoteDto
{
    [Required, MinLength(3), MaxLength(2000)]
    public string Note { get; set; } = string.Empty;
}

public class RepairNoteResponseDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public string MechanicId { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
