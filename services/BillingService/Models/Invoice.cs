namespace BillingService.Models;

public class Invoice
{
    public int Id { get; set; }
    public int JobCardId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public bool IsBillingEligible { get; set; }
    public bool IsGenerated { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ChargeLine> ChargeLines { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}
