namespace BillingService.Models;

public enum PaymentStatus { Unpaid, PartiallyPaid, Paid }

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Invoice? Invoice { get; set; }
}
