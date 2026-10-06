using System.Text.Json;
using BillingService.Data;
using BillingService.Events;
using BillingService.Models;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class PartIssuedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<PartIssuedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield immediately so Kafka connectivity can never block ASP.NET/Azure startup.
        await Task.Yield();
        var bootstrap = configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrap)) { logger.LogError("Kafka:BootstrapServers is missing. PartIssued consumer will not start."); return; }
        using var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig { BootstrapServers = bootstrap, GroupId = "billing-group", AutoOffsetReset = AutoOffsetReset.Earliest, EnableAutoCommit = false }).Build(); consumer.Subscribe("vsc.parts.issued");
        while (!stoppingToken.IsCancellationRequested)
        {
            try { var result = consumer.Consume(stoppingToken); await ProcessAsync(result.Message.Value, stoppingToken); consumer.Commit(result); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "PartIssued processing failed; the message was not committed and can be retried."); }
        }
        consumer.Close();
    }
    private async Task ProcessAsync(string payload, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<PartIssuedEvent>(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new JsonException("PartIssued payload is empty.");
        if (evt.EventId == Guid.Empty || !string.Equals(evt.EventType, "PartIssued", StringComparison.OrdinalIgnoreCase) || evt.Data.IssueId <= 0 || evt.Data.JobCardId <= 0 || evt.Data.SparePartId <= 0 || evt.Data.QuantityIssued <= 0 || evt.Data.UnitPrice < 0) throw new JsonException("Invalid PartIssued event.");
        using var scope = scopeFactory.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        if (await db.ProcessedKafkaEvents.AnyAsync(x => x.EventId == evt.EventId, ct)) { logger.LogInformation("Ignoring duplicate PartIssued event {EventId}.", evt.EventId); return; }
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var invoices = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        await invoices.AddPartAsync(evt.Data.JobCardId, evt.Data.IssueId, evt.Data.RequestId, evt.Data.SparePartId, evt.Data.SparePartName, evt.Data.QuantityIssued, evt.Data.UnitPrice, ct);
        db.ProcessedKafkaEvents.Add(new ProcessedKafkaEvent { EventId = evt.EventId, EventType = evt.EventType }); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        logger.LogInformation("Created part charge from PartIssued event {EventId} for JobCardId {JobCardId}.", evt.EventId, evt.Data.JobCardId);
    }
}
