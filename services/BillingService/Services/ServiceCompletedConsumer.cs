using System.Text.Json;
using BillingService.Data;
using BillingService.Events;
using BillingService.Models;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class ServiceCompletedConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<ServiceCompletedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            logger.LogError("Kafka:BootstrapServers is missing. ServiceCompleted consumer will not start.");
            return;
        }

        using var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "billing-group",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe("vsc.service.completed");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var evt = JsonSerializer.Deserialize<ServiceCompletedEvent>(result.Message.Value,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? throw new JsonException("ServiceCompleted payload is empty.");

                if (evt.EventId == Guid.Empty ||
                    !string.Equals(evt.EventType, "ServiceCompleted", StringComparison.OrdinalIgnoreCase) ||
                    evt.Data.JobCardId <= 0 || evt.Data.CustomerId <= 0 || evt.Data.VehicleId <= 0 ||
                    string.IsNullOrWhiteSpace(evt.Data.VehicleRegistrationNumber))
                    throw new JsonException("Invalid ServiceCompleted event.");

                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
                if (!await db.ProcessedKafkaEvents.AnyAsync(x => x.EventId == evt.EventId, stoppingToken))
                {
                    var invoice = await db.Invoices.SingleOrDefaultAsync(x => x.JobCardId == evt.Data.JobCardId, stoppingToken);
                    if (invoice is null)
                    {
                        invoice = new Invoice { JobCardId = evt.Data.JobCardId };
                        db.Invoices.Add(invoice);
                    }

                    invoice.JobCardNumber = evt.Data.JobCardNumber;
                    invoice.CustomerId = evt.Data.CustomerId;
                    invoice.VehicleId = evt.Data.VehicleId;
                    invoice.VehicleRegistrationNumber = evt.Data.VehicleRegistrationNumber;
                    invoice.IsBillingEligible = true;
                    invoice.UpdatedAt = DateTime.UtcNow;
                    db.ProcessedKafkaEvents.Add(new ProcessedKafkaEvent { EventId = evt.EventId, EventType = evt.EventType });
                    await db.SaveChangesAsync(stoppingToken);
                }

                consumer.Commit(result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ServiceCompleted processing failed; the message was not committed and can be retried.");
            }
        }

        consumer.Close();
    }
}
