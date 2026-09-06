using System.Text.Json;
using Confluent.Kafka;
using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IInspectionService
{
    Task<InspectionResponseDto> SaveAsync(CreateInspectionDto dto, string mechanicId, string mechanicName, CancellationToken cancellationToken = default);
    Task<List<InspectionResponseDto>> GetMyAsync(string mechanicId, CancellationToken cancellationToken = default);
    Task<List<InspectionResponseDto>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<InspectionResponseDto?> GetByJobAsync(int jobCardId, CancellationToken cancellationToken = default);
    Task<InspectionResponseDto> CompleteAsync(int inspectionId, string mechanicId, CancellationToken cancellationToken = default);
}

public class InspectionService : IInspectionService
{
    private const string Topic = "vsc.inspection.completed";
    private readonly JobMaintenanceDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(JobMaintenanceDbContext db, IConfiguration configuration, ILogger<InspectionService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<InspectionResponseDto> SaveAsync(CreateInspectionDto dto, string mechanicId, string mechanicName, CancellationToken cancellationToken = default)
    {
        ValidateText(dto.InspectionResults, nameof(dto.InspectionResults));
        ValidateText(dto.IdentifiedProblems, nameof(dto.IdentifiedProblems));

        var job = await _db.JobCards.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.JobCardId, cancellationToken);
        if (job is null) throw new KeyNotFoundException("Job card not found.");

        var assignment = await _db.MechanicAssignments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.JobCardId == dto.JobCardId && x.MechanicId == mechanicId && x.IsActive, cancellationToken);
        if (assignment is null) throw new UnauthorizedAccessException("You are not assigned to this job card.");

        var inspection = await _db.Inspections.FirstOrDefaultAsync(x => x.JobCardId == dto.JobCardId, cancellationToken);
        if (inspection?.IsCompleted == true) throw new InvalidOperationException("This inspection is already completed.");

        inspection ??= new Inspection { JobCardId = dto.JobCardId, MechanicId = mechanicId, MechanicName = mechanicName, CreatedAt = DateTime.UtcNow };
        inspection.InspectionResults = dto.InspectionResults.Trim();
        inspection.IdentifiedProblems = dto.IdentifiedProblems.Trim();
        inspection.MechanicId = mechanicId;
        inspection.MechanicName = mechanicName;

        if (inspection.Id == 0) _db.Inspections.Add(inspection);
        await _db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(inspection.Id, cancellationToken);
    }

    public async Task<List<InspectionResponseDto>> GetMyAsync(string mechanicId, CancellationToken cancellationToken = default) =>
        await Query().Where(x => x.MechanicId == mechanicId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<List<InspectionResponseDto>> GetCompletedAsync(CancellationToken cancellationToken = default) =>
        await Query().Where(x => x.IsCompleted).OrderByDescending(x => x.CompletedAt).ToListAsync(cancellationToken);

    public async Task<InspectionResponseDto?> GetByJobAsync(int jobCardId, CancellationToken cancellationToken = default) =>
        await Query().FirstOrDefaultAsync(x => x.JobCardId == jobCardId, cancellationToken);

    public async Task<InspectionResponseDto> CompleteAsync(int inspectionId, string mechanicId, CancellationToken cancellationToken = default)
    {
        var inspection = await _db.Inspections.FirstOrDefaultAsync(x => x.Id == inspectionId, cancellationToken);
        if (inspection is null) throw new KeyNotFoundException("Inspection not found.");
        if (inspection.MechanicId != mechanicId) throw new UnauthorizedAccessException("You can only complete your own inspection.");
        if (inspection.IsCompleted) return await BuildResponseAsync(inspection.Id, cancellationToken);

        ValidateText(inspection.InspectionResults, nameof(inspection.InspectionResults));
        ValidateText(inspection.IdentifiedProblems, nameof(inspection.IdentifiedProblems));

        var job = await _db.JobCards.FirstOrDefaultAsync(x => x.Id == inspection.JobCardId, cancellationToken);
        if (job is null) throw new KeyNotFoundException("Job card not found.");

        inspection.IsCompleted = true;
        inspection.CompletedAt = DateTime.UtcNow;
        inspection.CompletionEventId = Guid.NewGuid();
        job.Status = "Inspected";
        job.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await PublishCompletedAsync(inspection, job, cancellationToken);
        return await BuildResponseAsync(inspection.Id, cancellationToken);
    }

    private IQueryable<InspectionResponseDto> Query() =>
        from inspection in _db.Inspections.AsNoTracking()
        join job in _db.JobCards.AsNoTracking() on inspection.JobCardId equals job.Id
        select new InspectionResponseDto
        {
            Id = inspection.Id, JobCardId = inspection.JobCardId, JobCardNumber = job.JobCardNumber,
            VehicleRegistrationNumber = job.VehicleRegistrationNumber, MechanicId = inspection.MechanicId,
            MechanicName = inspection.MechanicName, InspectionResults = inspection.InspectionResults,
            IdentifiedProblems = inspection.IdentifiedProblems, IsCompleted = inspection.IsCompleted,
            CreatedAt = inspection.CreatedAt, CompletedAt = inspection.CompletedAt, CompletionEventId = inspection.CompletionEventId
        };

    private async Task<InspectionResponseDto> BuildResponseAsync(int id, CancellationToken cancellationToken) =>
        await Query().FirstAsync(x => x.Id == id, cancellationToken);

    private async Task PublishCompletedAsync(Inspection inspection, JobCard job, CancellationToken cancellationToken)
    {
        var bootstrap = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var evt = new InspectionCompletedEvent
        {
            EventId = inspection.CompletionEventId!.Value,
            OccurredAt = inspection.CompletedAt!.Value,
            CorrelationId = job.JobCardNumber,
            Data = new InspectionCompletedEventData
            {
                InspectionId = inspection.Id, JobCardId = job.Id, JobCardNumber = job.JobCardNumber,
                MechanicId = inspection.MechanicId, MechanicName = inspection.MechanicName,
                InspectionResults = inspection.InspectionResults, IdentifiedProblems = inspection.IdentifiedProblems
            }
        };

        var config = new ProducerConfig { BootstrapServers = bootstrap, Acks = Acks.All };
        try
        {
            using var producer = new ProducerBuilder<Null, string>(config).Build();
            var message = new Message<Null, string> { Value = JsonSerializer.Serialize(evt) };
            await producer.ProduceAsync(Topic, message, cancellationToken);
            _logger.LogInformation("Published InspectionCompleted event {EventId} for job {JobCardId}.", evt.EventId, job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inspection completed but Kafka publication failed for event {EventId}.", evt.EventId);
            throw;
        }
    }

    private static void ValidateText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 3)
            throw new ArgumentException($"{fieldName} is required and must contain at least 3 characters.");
    }
}
