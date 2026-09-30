namespace BillingService.Events;

public class InvoiceGeneratedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = "InvoiceGenerated";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "BillingService";
    public string? CorrelationId { get; set; }
    public InvoiceGeneratedData Data { get; set; } = new();
}

public class InvoiceGeneratedData
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int JobCardId { get; set; }
    public int CustomerId { get; set; }
    public decimal InvoiceTotal { get; set; }
}
