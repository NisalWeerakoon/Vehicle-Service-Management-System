namespace BillingService.Models;

public class PartCharge
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public int PartIssueId { get; set; }
    public int PartRequestId { get; set; }
    public int SparePartId { get; set; }
    public string SparePartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
