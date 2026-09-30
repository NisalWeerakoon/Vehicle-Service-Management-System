namespace InventoryService.Events;

public class LowStockDetectedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = "LowStockDetected";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "InventoryService";
    public LowStockDetectedEventData Data { get; set; } = new();
}

public class LowStockDetectedEventData
{
    public int SparePartId { get; set; }
    public string SparePartName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int LowStockThreshold { get; set; }
}
