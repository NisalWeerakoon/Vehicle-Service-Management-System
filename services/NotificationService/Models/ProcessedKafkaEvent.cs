using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models;

public class ProcessedKafkaEvent
{
    public int Id { get; set; }

    public Guid EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
