using BillingService.Models;

namespace BillingService.Events;

public class PaymentRecordedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = "PaymentRecorded";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "BillingService";
    public string? CorrelationId { get; set; }
    public PaymentRecordedData Data { get; set; } = new();
}

public class PaymentRecordedData
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int JobCardId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}
