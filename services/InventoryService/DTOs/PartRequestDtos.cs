using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTOs;

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
    public string SparePartName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int CurrentStock { get; set; }
    public string RequestingMechanicId { get; set; } = string.Empty;
    public string RequestingMechanicName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public PartIssueResponseDto? Issue { get; set; }
}

public class PartIssueResponseDto
{
    public int Id { get; set; }
    public int PartRequestId { get; set; }
    public int JobCardId { get; set; }
    public int SparePartId { get; set; }
    public int QuantityIssued { get; set; }
    public string InventoryOfficerId { get; set; } = string.Empty;
    public string InventoryOfficerName { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
}
