using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IRepairTaskService
{
    Task<List<RepairTaskResponseDto>> GetByJobAsync(int jobCardId, string? mechanicId, bool isStaff, CancellationToken cancellationToken = default);
    Task<RepairTaskResponseDto> CreateAsync(CreateRepairTaskDto dto, string mechanicId, string mechanicName, CancellationToken cancellationToken = default);
    Task<RepairTaskResponseDto> UpdateAsync(int id, UpdateRepairTaskDto dto, string mechanicId, CancellationToken cancellationToken = default);
    Task<RepairTaskResponseDto> CompleteAsync(int id, string mechanicId, CancellationToken cancellationToken = default);
}

public class RepairTaskService : IRepairTaskService
{
    private readonly JobMaintenanceDbContext _db;

    public RepairTaskService(JobMaintenanceDbContext db) => _db = db;

    public async Task<List<RepairTaskResponseDto>> GetByJobAsync(int jobCardId, string? mechanicId, bool isStaff, CancellationToken cancellationToken = default)
    {
        await EnsureJobExistsAsync(jobCardId, cancellationToken);
        if (!isStaff && !await IsAssignedAsync(jobCardId, mechanicId!, cancellationToken))
            throw new UnauthorizedAccessException("You are not assigned to this job card.");

        return await Query().Where(x => x.JobCardId == jobCardId)
            .OrderBy(x => x.IsCompleted).ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RepairTaskResponseDto> CreateAsync(CreateRepairTaskDto dto, string mechanicId, string mechanicName, CancellationToken cancellationToken = default)
    {
        Validate(dto.TaskTitle, nameof(dto.TaskTitle), 3, 200);
        Validate(dto.TaskDescription, nameof(dto.TaskDescription), 3, 1000);
        await EnsureAssignedAsync(dto.JobCardId, mechanicId, cancellationToken);

        var task = new RepairTask
        {
            JobCardId = dto.JobCardId, MechanicId = mechanicId, MechanicName = mechanicName,
            TaskTitle = dto.TaskTitle.Trim(), TaskDescription = dto.TaskDescription.Trim(), CreatedAt = DateTime.UtcNow
        };
        _db.RepairTasks.Add(task);
        await SetJobInProgressAsync(dto.JobCardId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(task.Id, cancellationToken);
    }

    public async Task<RepairTaskResponseDto> UpdateAsync(int id, UpdateRepairTaskDto dto, string mechanicId, CancellationToken cancellationToken = default)
    {
        Validate(dto.TaskTitle, nameof(dto.TaskTitle), 3, 200);
        Validate(dto.TaskDescription, nameof(dto.TaskDescription), 3, 1000);
        var task = await _db.RepairTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Repair task not found.");
        await EnsureAssignedAsync(task.JobCardId, mechanicId, cancellationToken);
        if (task.IsCompleted) throw new InvalidOperationException("Completed repair tasks cannot be edited.");

        task.TaskTitle = dto.TaskTitle.Trim(); task.TaskDescription = dto.TaskDescription.Trim(); task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(id, cancellationToken);
    }

    public async Task<RepairTaskResponseDto> CompleteAsync(int id, string mechanicId, CancellationToken cancellationToken = default)
    {
        var task = await _db.RepairTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Repair task not found.");
        await EnsureAssignedAsync(task.JobCardId, mechanicId, cancellationToken);
        if (!task.IsCompleted)
        {
            task.IsCompleted = true; task.CompletedAt = DateTime.UtcNow; task.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        return await BuildResponseAsync(id, cancellationToken);
    }

    private IQueryable<RepairTaskResponseDto> Query() =>
        from task in _db.RepairTasks.AsNoTracking()
        join job in _db.JobCards.AsNoTracking() on task.JobCardId equals job.Id
        select new RepairTaskResponseDto
        {
            Id = task.Id, JobCardId = task.JobCardId, JobCardNumber = job.JobCardNumber,
            VehicleRegistrationNumber = job.VehicleRegistrationNumber, MechanicId = task.MechanicId,
            MechanicName = task.MechanicName, TaskTitle = task.TaskTitle, TaskDescription = task.TaskDescription,
            IsCompleted = task.IsCompleted, CreatedAt = task.CreatedAt, UpdatedAt = task.UpdatedAt, CompletedAt = task.CompletedAt
        };

    private async Task<RepairTaskResponseDto> BuildResponseAsync(int id, CancellationToken cancellationToken) => await Query().FirstAsync(x => x.Id == id, cancellationToken);

    private async Task EnsureAssignedAsync(int jobCardId, string mechanicId, CancellationToken cancellationToken)
    {
        await EnsureJobExistsAsync(jobCardId, cancellationToken);
        if (!await IsAssignedAsync(jobCardId, mechanicId, cancellationToken)) throw new UnauthorizedAccessException("You are not assigned to this job card.");
    }

    private async Task<bool> IsAssignedAsync(int jobCardId, string mechanicId, CancellationToken cancellationToken) =>
        await _db.MechanicAssignments.AnyAsync(x => x.JobCardId == jobCardId && x.MechanicId == mechanicId && x.IsActive, cancellationToken);

    private async Task EnsureJobExistsAsync(int jobCardId, CancellationToken cancellationToken)
    {
        if (!await _db.JobCards.AnyAsync(x => x.Id == jobCardId, cancellationToken)) throw new KeyNotFoundException("Job card not found.");
    }

    private async Task SetJobInProgressAsync(int jobCardId, CancellationToken cancellationToken)
    {
        var job = await _db.JobCards.FirstAsync(x => x.Id == jobCardId, cancellationToken);
        if (!string.Equals(job.Status, "Completed", StringComparison.OrdinalIgnoreCase) && !string.Equals(job.Status, "Closed", StringComparison.OrdinalIgnoreCase))
        { job.Status = "In Progress"; job.UpdatedAt = DateTime.UtcNow; }
    }

    private static void Validate(string? value, string field, int min, int max)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length < min) throw new ArgumentException($"{field} is required and must contain at least {min} characters.");
        if (text.Length > max) throw new ArgumentException($"{field} cannot exceed {max} characters.");
    }
}
