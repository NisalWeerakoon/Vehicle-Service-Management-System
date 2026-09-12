using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IActiveJobsReportService
{
    Task<ActiveJobsReportDto> GetActiveJobsAsync(
        string? status,
        CancellationToken cancellationToken = default);
}

public class ActiveJobsReportService : IActiveJobsReportService
{
    private static readonly string[] ActiveStatuses =
    {
        "Created",
        "Awaiting Inspection",
        "Inspected",
        "Awaiting Parts",
        "In Progress",
        "Completed",
        "Ready for Collection"
    };

    private readonly JobMaintenanceDbContext _db;

    public ActiveJobsReportService(JobMaintenanceDbContext db)
    {
        _db = db;
    }

    public async Task<ActiveJobsReportDto> GetActiveJobsAsync(
        string? status,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        if (normalizedStatus is not null &&
            !ActiveStatuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Unsupported status '{normalizedStatus}'.",
                nameof(status));
        }

        // Read-only query with projection keeps the report lightweight.
        // Closed jobs are not part of the active operational workload.
        var query = _db.JobCards
            .AsNoTracking()
            .Where(j => j.Status != "Closed");

        if (normalizedStatus is not null)
        {
            query = query.Where(j => j.Status == normalizedStatus);
        }

        var jobs = await query
            .OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt)
            .Select(j => new ActiveJobReportItemDto
            {
                Id = j.Id,
                JobCardNumber = j.JobCardNumber,
                CheckInId = j.CheckInId,
                CustomerId = j.CustomerId,
                VehicleId = j.VehicleId,
                VehicleRegistrationNumber = j.VehicleRegistrationNumber,
                ReportedProblems = j.ReportedProblems,
                Status = j.Status,
                CreatedAt = j.CreatedAt,
                UpdatedAt = j.UpdatedAt,
                AssignedMechanicId = _db.MechanicAssignments
                    .Where(a => a.JobCardId == j.Id && a.IsActive)
                    .Select(a => a.MechanicId)
                    .FirstOrDefault(),
                AssignedMechanicName = _db.MechanicAssignments
                    .Where(a => a.JobCardId == j.Id && a.IsActive)
                    .Select(a => a.MechanicName)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        // Keep the status order consistent with the Story 6 workflow.
        var counts = jobs
            .GroupBy(j => j.Status)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var statusCounts = ActiveStatuses
            .Select(s => new ActiveJobStatusCountDto
            {
                Status = s,
                Count = counts.TryGetValue(s, out var count) ? count : 0
            })
            .ToList();

        return new ActiveJobsReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            TotalActiveJobs = jobs.Count,
            AppliedStatusFilter = normalizedStatus,
            StatusCounts = statusCounts,
            Jobs = jobs
        };
    }

    public static IReadOnlyList<string> GetActiveStatuses() => ActiveStatuses;
}
