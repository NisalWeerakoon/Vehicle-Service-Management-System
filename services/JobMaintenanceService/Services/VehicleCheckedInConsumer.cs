using System.Text.Json;
using Confluent.Kafka;
using JobMaintenanceService.Data;
using JobMaintenanceService.Events;
using JobMaintenanceService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public class VehicleCheckedInConsumer : BackgroundService
{
    private const string TopicName = "vsc.vehicle.checked-in";
    private const string ConsumerGroup = "job-maintenance-group";

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VehicleCheckedInConsumer> _logger;

    public VehicleCheckedInConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<VehicleCheckedInConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogError("Kafka:BootstrapServers is missing. VehicleCheckedIn consumer will not start.");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(TopicName);

        _logger.LogInformation("Subscribed to Kafka topic {Topic} with group {Group}", TopicName, ConsumerGroup);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    await ProcessMessageAsync(result.Message.Value, stoppingToken);
                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error for {Topic}", TopicName);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid VehicleCheckedIn event payload. The message will not be committed.");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing VehicleCheckedIn event");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(string payload, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var eventMessage = JsonSerializer.Deserialize<VehicleCheckedInEvent>(payload, options)
            ?? throw new JsonException("VehicleCheckedIn event payload is empty.");

        if (eventMessage.EventId == Guid.Empty)
            throw new JsonException("VehicleCheckedIn eventId is missing.");

        if (!string.Equals(eventMessage.EventType, "VehicleCheckedIn", StringComparison.OrdinalIgnoreCase))
            throw new JsonException($"Unexpected event type: {eventMessage.EventType}");

        if (eventMessage.Data.CheckInId <= 0 || eventMessage.Data.CustomerId <= 0 || eventMessage.Data.VehicleId <= 0)
            throw new JsonException("VehicleCheckedIn event contains invalid identifiers.");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobMaintenanceDbContext>();

        var alreadyProcessed = await db.ProcessedKafkaEvents
            .AnyAsync(x => x.EventId == eventMessage.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation("Ignoring duplicate Kafka event {EventId}", eventMessage.EventId);
            return;
        }

        var existingJob = await db.JobCards
            .FirstOrDefaultAsync(x => x.CheckInId == eventMessage.Data.CheckInId, cancellationToken);

        if (existingJob is null)
        {
            existingJob = new Models.JobCard
            {
                JobCardNumber = $"JC-{Guid.NewGuid():N}"[..19].ToUpperInvariant(),
                CheckInId = eventMessage.Data.CheckInId,
                CustomerId = eventMessage.Data.CustomerId,
                VehicleId = eventMessage.Data.VehicleId,
                VehicleRegistrationNumber = eventMessage.Data.VehicleRegistrationNumber.Trim().ToUpperInvariant(),
                ReportedProblems = eventMessage.Data.ReportedProblems.Trim(),
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            db.JobCards.Add(existingJob);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created Job Card {JobCardNumber} for CheckInId {CheckInId}",
                existingJob.JobCardNumber,
                existingJob.CheckInId);
        }
        else
        {
            _logger.LogInformation(
                "Job Card {JobCardNumber} already exists for CheckInId {CheckInId}",
                existingJob.JobCardNumber,
                existingJob.CheckInId);
        }

        db.ProcessedKafkaEvents.Add(new Models.ProcessedKafkaEvent
        {
            EventId = eventMessage.EventId,
            EventType = eventMessage.EventType,
            ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
