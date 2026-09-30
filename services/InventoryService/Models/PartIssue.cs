namespace InventoryService.Models;

public class PartIssue
{
    public int Id { get; set; }
    public int PartRequestId { get; set; }
    public int JobCardId { get; set; }
    public int SparePartId { get; set; }
    public int QuantityIssued { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string InventoryOfficerId { get; set; } = string.Empty;
    public string InventoryOfficerName { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public PartRequest? PartRequest { get; set; }
    public SparePart? SparePart { get; set; }
}
