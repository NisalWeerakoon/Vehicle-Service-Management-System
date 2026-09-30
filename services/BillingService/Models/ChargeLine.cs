namespace BillingService.Models;

public enum ChargeType { Service, Labour, Part }

public class ChargeLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public ChargeType ChargeType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? SourceReferenceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Invoice? Invoice { get; set; }
}
