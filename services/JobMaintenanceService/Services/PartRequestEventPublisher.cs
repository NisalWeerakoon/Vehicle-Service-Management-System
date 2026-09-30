using System.Text.Json;
using Confluent.Kafka;
using JobMaintenanceService.Events;

namespace JobMaintenanceService.Services;

public interface IPartRequestEventPublisher { Task PublishAsync(PartRequestedEvent evt, CancellationToken ct = default); }

public class PartRequestEventPublisher(IConfiguration configuration, ILogger<PartRequestEventPublisher> logger) : IPartRequestEventPublisher
{
    private const string Topic = "vsc.parts.requested";
    public async Task PublishAsync(PartRequestedEvent evt, CancellationToken ct = default)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing.");
        using var producer = new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = bootstrap, Acks = Acks.All }).Build();
        try
        {
            await producer.ProduceAsync(Topic, new Message<Null, string> { Value = JsonSerializer.Serialize(evt) }, ct);
            logger.LogInformation("Published PartRequested event {EventId} for request {RequestId}.", evt.EventId, evt.Data.RequestId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish PartRequested event {EventId} for request {RequestId}.", evt.EventId, evt.Data.RequestId);
            throw;
        }
    }
}
