namespace InventoryService.Models;

public class ProcessedKafkaEvent
{
    public int Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
