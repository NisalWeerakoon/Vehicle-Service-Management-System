using System.ComponentModel.DataAnnotations;

namespace JobMaintenanceService.DTOs;

public class CreatePartRequestDto
{
    [Range(1, int.MaxValue)] public int JobCardId { get; set; }
    [Range(1, int.MaxValue)] public int SparePartId { get; set; }
    [Range(1, int.MaxValue)] public int RequestedQuantity { get; set; }
}

public class PartRequestResponseDto
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int SparePartId { get; set; }
    public int RequestedQuantity { get; set; }
    public string RequestingMechanicId { get; set; } = string.Empty;
    public string RequestingMechanicName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; }
}
