using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class AssignMechanicDto
{
    [Range(1, int.MaxValue)]
    public int JobCardId { get; set; }

    [Required]
    public string MechanicId { get; set; } = string.Empty;
}

public class MechanicAssignmentResponseDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string MechanicId { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
}

public class StaffValidationResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
