using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Data;
using InventoryService.Events;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services;

public class PartRequestedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<PartRequestedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrap)) { logger.LogError("Kafka:BootstrapServers is missing. PartRequested consumer will not start."); return; }
        using var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig { BootstrapServers = bootstrap, GroupId = "inventory-group", AutoOffsetReset = AutoOffsetReset.Earliest, EnableAutoCommit = false }).Build();
        consumer.Subscribe("vsc.parts.requested");
        while (!stoppingToken.IsCancellationRequested)
        {
            try { var result = consumer.Consume(stoppingToken); await ProcessAsync(result.Message.Value, stoppingToken); consumer.Commit(result); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "PartRequested processing failed; the message was not committed and can be retried."); }
        }
        consumer.Close();
    }

    private async Task ProcessAsync(string payload, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<PartRequestedEvent>(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new JsonException("PartRequested payload is empty.");
        if (evt.EventId == Guid.Empty || !string.Equals(evt.EventType, "PartRequested", StringComparison.OrdinalIgnoreCase) || evt.Data.RequestId <= 0 || evt.Data.JobCardId <= 0 || evt.Data.SparePartId <= 0 || evt.Data.RequestedQuantity <= 0) throw new JsonException("Invalid PartRequested event.");
        using var scope = scopeFactory.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        if (await db.ProcessedKafkaEvents.AnyAsync(x => x.EventId == evt.EventId, ct)) { logger.LogInformation("Ignoring duplicate PartRequested event {EventId}.", evt.EventId); return; }
        var part = await db.SpareParts.FirstOrDefaultAsync(x => x.Id == evt.Data.SparePartId, ct) ?? throw new InvalidOperationException("Spare part in event does not exist.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (!await db.PartRequests.AnyAsync(x => x.SourceRequestId == evt.Data.RequestId, ct))
            db.PartRequests.Add(new PartRequest { SourceRequestId = evt.Data.RequestId, JobCardId = evt.Data.JobCardId, JobCardNumber = evt.Data.JobCardNumber, SparePartId = part.Id, RequestedQuantity = evt.Data.RequestedQuantity, RequestingMechanicId = evt.Data.RequestingMechanicId, RequestingMechanicName = evt.Data.RequestingMechanicName, Status = PartRequestStatus.Pending, RequestedAt = evt.OccurredAt });
        db.ProcessedKafkaEvents.Add(new ProcessedKafkaEvent { EventId = evt.EventId, EventType = evt.EventType });
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        logger.LogInformation("Processed PartRequested event {EventId} for JobCardId {JobCardId}.", evt.EventId, evt.Data.JobCardId);
    }
}
