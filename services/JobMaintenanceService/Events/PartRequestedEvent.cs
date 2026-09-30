namespace JobMaintenanceService.Events;

public class PartRequestedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = "PartRequested";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "JobMaintenanceService";
    public string? CorrelationId { get; set; }
    public PartRequestedData Data { get; set; } = new();
}

public class PartRequestedData
{
    public int RequestId { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int SparePartId { get; set; }
    public int RequestedQuantity { get; set; }
    public string RequestingMechanicId { get; set; } = string.Empty;
    public string RequestingMechanicName { get; set; } = string.Empty;
}
