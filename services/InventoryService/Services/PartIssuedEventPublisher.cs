using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Events;

namespace InventoryService.Services;

public interface IPartIssuedEventPublisher { Task PublishAsync(PartIssuedEvent evt, CancellationToken ct = default); }
public class PartIssuedEventPublisher(IConfiguration configuration, ILogger<PartIssuedEventPublisher> logger) : IPartIssuedEventPublisher
{
    public async Task PublishAsync(PartIssuedEvent evt, CancellationToken ct = default)
    {
        using var producer = new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing."), Acks = Acks.All }).Build();
        try { await producer.ProduceAsync("vsc.parts.issued", new Message<Null, string> { Value = JsonSerializer.Serialize(evt) }, ct); logger.LogInformation("Published PartIssued event {EventId} for issue {IssueId}.", evt.EventId, evt.Data.IssueId); }
        catch (Exception ex) { logger.LogError(ex, "Failed to publish PartIssued event {EventId} for issue {IssueId}.", evt.EventId, evt.Data.IssueId); throw; }
    }
}
