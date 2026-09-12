using System.Text.Json;
using Confluent.Kafka;
using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Events;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IJobStatusService
{
    Task<JobStatusResponseDto> GetStatusAsync(int jobCardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobStatusHistoryDto>> GetHistoryAsync(int jobCardId, CancellationToken cancellationToken = default);
    Task<JobStatusResponseDto> UpdateStatusAsync(
        int jobCardId,
        string newStatus,
        string userId,
        string role,
        CancellationToken cancellationToken = default);
}

public class JobStatusService : IJobStatusService
{
    public const string AwaitingInspection = "Awaiting Inspection";
    public const string Inspected = "Inspected";
    public const string AwaitingParts = "Awaiting Parts";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string ReadyForCollection = "Ready for Collection";
    public const string Closed = "Closed";

    private readonly JobMaintenanceDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JobStatusService> _logger;

    private static readonly IReadOnlyDictionary<string, string[]> ValidTransitions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Created"] = [AwaitingInspection],
            [AwaitingInspection] = [Inspected],
            [Inspected] = [AwaitingParts, InProgress],
            [AwaitingParts] = [InProgress],
            [InProgress] = [Completed],
            [Completed] = [ReadyForCollection],
            [ReadyForCollection] = [Closed],
            [Closed] = []
        };

    public JobStatusService(
        JobMaintenanceDbContext db,
        IConfiguration configuration,
        ILogger<JobStatusService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<JobStatusResponseDto> GetStatusAsync(
        int jobCardId,
        CancellationToken cancellationToken = default)
    {
        var job = await _db.JobCards.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == jobCardId, cancellationToken);

        if (job is null)
            throw new KeyNotFoundException("Job card not found.");

        return ToResponse(job);
    }

    public async Task<IReadOnlyList<JobStatusHistoryDto>> GetHistoryAsync(
        int jobCardId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.JobCards.AsNoTracking()
            .AnyAsync(x => x.Id == jobCardId, cancellationToken);

        if (!exists)
            throw new KeyNotFoundException("Job card not found.");

        try
        {
            return await _db.JobStatusHistories.AsNoTracking()
                .Where(x => x.JobCardId == jobCardId)
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new JobStatusHistoryDto
                {
                    Id = x.Id,
                    JobCardId = x.JobCardId,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ChangedBy = x.ChangedBy,
                    ChangedByRole = x.ChangedByRole,
                    ChangedAt = x.ChangedAt
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve status history for job card {JobCardId}", jobCardId);
            return new List<JobStatusHistoryDto>();
        }
    }

    public async Task<JobStatusResponseDto> UpdateStatusAsync(
        int jobCardId,
        string newStatus,
        string userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        newStatus = newStatus?.Trim() ?? string.Empty;
        userId = userId?.Trim() ?? string.Empty;
        role = role?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Status is required.");

        var job = await _db.JobCards
            .FirstOrDefaultAsync(x => x.Id == jobCardId, cancellationToken);

        if (job is null)
            throw new KeyNotFoundException("Job card not found.");

        if (!IsAuthorizedRole(role))
            throw new UnauthorizedAccessException("You are not authorized to change job status.");

        if (role.Equals("Mechanic", StringComparison.OrdinalIgnoreCase))
        {
            var hasAssignments = await _db.MechanicAssignments.AsNoTracking()
                .AnyAsync(x => x.JobCardId == jobCardId && x.IsActive, cancellationToken);

            if (hasAssignments)
            {
                var assigned = await _db.MechanicAssignments.AsNoTracking()
                    .AnyAsync(
                        x => x.JobCardId == jobCardId &&
                             (x.MechanicId == userId || x.MechanicName == userId || x.MechanicName.ToLower() == userId.ToLower()) &&
                             x.IsActive,
                        cancellationToken);

                if (!assigned)
                {
                    _logger.LogInformation(
                        "Mechanic {UserId} transitioning status for job card {JobCardId}", userId, jobCardId);
                }
            }
        }

        var allowed = GetAllowedNextStatuses(job.Status);

        if (!allowed.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Invalid status transition from '{job.Status}' to '{newStatus}'.");

        var now = DateTime.UtcNow;
        var previousStatus = job.Status;
        job.Status = NormalizeStatus(newStatus);
        job.UpdatedAt = now;

        _db.JobStatusHistories.Add(new JobStatusHistory
        {
            JobCardId = job.Id,
            FromStatus = previousStatus,
            ToStatus = job.Status,
            ChangedBy = userId,
            ChangedByRole = role,
            ChangedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);

        if (job.Status.Equals(Completed, StringComparison.OrdinalIgnoreCase))
            await PublishServiceCompletedAsync(job, userId, now, cancellationToken);

        if (job.Status.Equals(ReadyForCollection, StringComparison.OrdinalIgnoreCase))
            await PublishVehicleReadyForCollectionAsync(job, userId, now, cancellationToken);

        return ToResponse(job);
    }

    private async Task PublishServiceCompletedAsync(
        JobCard job, string completedBy, DateTime completedAt, CancellationToken cancellationToken)
    {
        var evt = new ServiceCompletedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = completedAt,
            CorrelationId = job.JobCardNumber,
            Data = new ServiceCompletedData
            {
                JobCardId = job.Id,
                JobCardNumber = job.JobCardNumber,
                CustomerId = job.CustomerId,
                VehicleId = job.VehicleId,
                VehicleRegistrationNumber = job.VehicleRegistrationNumber,
                CompletedBy = completedBy,
                CompletedAt = completedAt
            }
        };

        await PublishAsync("vsc.service.completed", evt.EventId, evt, cancellationToken);
    }

    private async Task PublishVehicleReadyForCollectionAsync(
        JobCard job, string readyBy, DateTime readyAt, CancellationToken cancellationToken)
    {
        var evt = new VehicleReadyForCollectionEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = readyAt,
            CorrelationId = job.JobCardNumber,
            Data = new VehicleReadyForCollectionData
            {
                JobCardId = job.Id,
                JobCardNumber = job.JobCardNumber,
                CustomerId = job.CustomerId,
                VehicleId = job.VehicleId,
                VehicleRegistrationNumber = job.VehicleRegistrationNumber,
                ReadyBy = readyBy,
                ReadyAt = readyAt
            }
        };

        await PublishAsync(
            "vsc.vehicle.ready-for-collection",
            evt.EventId,
            evt,
            cancellationToken);
    }

    private async Task PublishAsync<T>(
        string topic, Guid eventId, T payload, CancellationToken cancellationToken)
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];

            if (string.IsNullOrWhiteSpace(bootstrapServers))
                return;

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                MessageTimeoutMs = 2000
            };

            var json = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            using var producer = new ProducerBuilder<string, string>(config).Build();

            await producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = eventId.ToString(),
                    Value = json
                },
                cancellationToken);

            _logger.LogInformation(
                "Published {EventType} event {EventId} to {Topic}",
                typeof(T).Name, eventId, topic);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kafka unavailable. Skipping publication of event {EventId} to {Topic}", eventId, topic);
        }
    }

    private static bool IsAuthorizedRole(string role) =>
        role.Equals("Mechanic", StringComparison.OrdinalIgnoreCase) ||
        role.Equals("ServiceAdvisor", StringComparison.OrdinalIgnoreCase) ||
        role.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

    public static string[] GetAllowedNextStatuses(string currentStatus) =>
        ValidTransitions.TryGetValue(currentStatus ?? string.Empty, out var next)
            ? next
            : [];

    private static string NormalizeStatus(string status)
    {
        var match = ValidTransitions.Keys
            .Concat(ValidTransitions.Values.SelectMany(x => x))
            .FirstOrDefault(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));

        return match ?? status;
    }

    private static JobStatusResponseDto ToResponse(JobCard job) => new()
    {
        JobCardId = job.Id,
        JobCardNumber = job.JobCardNumber,
        CurrentStatus = job.Status,
        AllowedNextStatuses = GetAllowedNextStatuses(job.Status).ToList()
    };
}
