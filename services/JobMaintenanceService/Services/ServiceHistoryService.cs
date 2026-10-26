using System.Text.Json;
using JobMaintenanceService.Data;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IServiceHistoryService
{
    Task<bool> CaptureCompletedAsync(
        JobCard job,
        string completedBy,
        DateTime completedAt,
        CancellationToken cancellationToken = default);
}

public class ServiceHistoryService(JobMaintenanceDbContext db) : IServiceHistoryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> CaptureCompletedAsync(
        JobCard job,
        string completedBy,
        DateTime completedAt,
        CancellationToken cancellationToken = default)
    {
        if (!job.Status.Equals(JobStatusService.Completed, StringComparison.OrdinalIgnoreCase))
            return false;

        var duplicateTracked = db.ServiceHistories.Local.Any(x => x.JobCardId == job.Id);
        var duplicatePersisted = await db.ServiceHistories.AsNoTracking()
            .AnyAsync(x => x.JobCardId == job.Id, cancellationToken);

        if (duplicateTracked || duplicatePersisted)
            return false;

        var completedTasks = await db.RepairTasks.AsNoTracking()
            .Where(x => x.JobCardId == job.Id && x.IsCompleted)
            .OrderBy(x => x.Id)
            .Select(x => new CompletedRepairTaskSnapshot(
                x.Id,
                x.TaskTitle,
                x.TaskDescription,
                x.CompletedAt))
            .ToListAsync(cancellationToken);

        var repairNotes = await db.RepairNotes.AsNoTracking()
            .Where(x => x.JobCardId == job.Id)
            .OrderBy(x => x.Id)
            .Select(x => new RepairNoteSnapshot(
                x.Id,
                x.Note,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var inspection = await db.Inspections.AsNoTracking()
            .Where(x => x.JobCardId == job.Id)
            .Select(x => new
            {
                x.InspectionResults,
                x.IdentifiedProblems,
                x.CompletedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        db.ServiceHistories.Add(new ServiceHistory
        {
            JobCardId = job.Id,
            JobCardNumber = job.JobCardNumber,
            VehicleId = job.VehicleId,
            VehicleRegistrationNumber = job.VehicleRegistrationNumber,
            CustomerId = job.CustomerId,
            ReportedProblems = job.ReportedProblems,
            CompletedBy = completedBy,
            FinalStatus = job.Status,
            CompletedAt = completedAt,
            CapturedAt = completedAt,
            CompletedRepairTasksJson = JsonSerializer.Serialize(completedTasks, JsonOptions),
            RepairNotesJson = JsonSerializer.Serialize(repairNotes, JsonOptions),
            InspectionResults = inspection?.InspectionResults,
            IdentifiedProblems = inspection?.IdentifiedProblems,
            InspectionCompletedAt = inspection?.CompletedAt
        });

        return true;
    }

    private sealed record CompletedRepairTaskSnapshot(
        int Id,
        string Title,
        string Description,
        DateTime? CompletedAt);

    private sealed record RepairNoteSnapshot(
        int Id,
        string Note,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
