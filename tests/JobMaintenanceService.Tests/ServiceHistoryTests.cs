using JobMaintenanceService.Data;
using JobMaintenanceService.Models;
using JobMaintenanceService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace JobMaintenanceService.Tests;

public class ServiceHistoryTests
{
    [Fact]
    public void Model_EnforcesOneHistoryPerJob_AndSupportsVehicleChronology()
    {
        using var db = CreateDbContext();
        var entity = db.Model.FindEntityType(typeof(ServiceHistory));

        Assert.NotNull(entity);
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(ServiceHistory.JobCardId)]));
        Assert.Contains(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(ServiceHistory.VehicleId), nameof(ServiceHistory.CompletedAt)]));
    }

    [Fact]
    public async Task CaptureCompletedAsync_CapturesSupportedSnapshot_AndPreventsDuplicates()
    {
        await using var db = CreateDbContext();
        var completedAt = new DateTime(2026, 10, 12, 8, 30, 0, DateTimeKind.Utc);
        var job = CreateJob(JobStatusService.Completed);
        db.Add(job);
        await db.SaveChangesAsync();

        db.RepairTasks.AddRange(
            new RepairTask
            {
                JobCardId = job.Id,
                MechanicId = "mechanic-1",
                MechanicName = "Mechanic One",
                TaskTitle = "Replace brake pads",
                TaskDescription = "Replaced worn front brake pads.",
                IsCompleted = true,
                CompletedAt = completedAt
            },
            new RepairTask
            {
                JobCardId = job.Id,
                MechanicId = "mechanic-1",
                MechanicName = "Mechanic One",
                TaskTitle = "Pending wheel alignment",
                TaskDescription = "Alignment was not completed.",
                IsCompleted = false
            });
        db.RepairNotes.Add(new RepairNote
        {
            JobCardId = job.Id,
            MechanicId = "mechanic-1",
            MechanicName = "Mechanic One",
            Note = "Road test completed successfully."
        });
        db.Inspections.Add(new Inspection
        {
            JobCardId = job.Id,
            MechanicId = "mechanic-1",
            MechanicName = "Mechanic One",
            InspectionResults = "Front brake pads were worn.",
            IdentifiedProblems = "Reduced front braking performance.",
            IsCompleted = true,
            CompletedAt = completedAt.AddHours(-1)
        });
        await db.SaveChangesAsync();

        var service = new ServiceHistoryService(db);

        Assert.True(await service.CaptureCompletedAsync(job, "advisor-1", completedAt));
        Assert.False(await service.CaptureCompletedAsync(job, "advisor-1", completedAt));
        await db.SaveChangesAsync();
        Assert.False(await service.CaptureCompletedAsync(job, "advisor-1", completedAt));

        var history = await db.ServiceHistories.SingleAsync();
        Assert.Equal(job.Id, history.JobCardId);
        Assert.Equal(job.VehicleId, history.VehicleId);
        Assert.Equal(job.CustomerId, history.CustomerId);
        Assert.Equal(completedAt, history.CompletedAt);
        Assert.Equal("advisor-1", history.CompletedBy);
        Assert.Equal(JobStatusService.Completed, history.FinalStatus);
        Assert.Contains("Replace brake pads", history.CompletedRepairTasksJson);
        Assert.DoesNotContain("Pending wheel alignment", history.CompletedRepairTasksJson);
        Assert.Contains("Road test completed successfully.", history.RepairNotesJson);
        Assert.Equal("Front brake pads were worn.", history.InspectionResults);
        Assert.Equal("Reduced front braking performance.", history.IdentifiedProblems);
    }

    [Fact]
    public async Task CaptureCompletedAsync_IncompleteJob_DoesNotCreateHistory()
    {
        await using var db = CreateDbContext();
        var job = CreateJob(JobStatusService.InProgress);
        db.Add(job);
        await db.SaveChangesAsync();

        var created = await new ServiceHistoryService(db)
            .CaptureCompletedAsync(job, "advisor-1", DateTime.UtcNow);

        Assert.False(created);
        Assert.Empty(db.ServiceHistories.Local);
        Assert.False(await db.ServiceHistories.AnyAsync());
    }

    [Fact]
    public async Task UpdateStatusAsync_CompletionPersistsHistoryAndStatusTogether()
    {
        await using var db = CreateDbContext();
        var job = CreateJob(JobStatusService.InProgress);
        db.Add(job);
        await db.SaveChangesAsync();

        var service = CreateJobStatusService(db, new ServiceHistoryService(db));
        var result = await service.UpdateStatusAsync(
            job.Id,
            JobStatusService.Completed,
            "advisor-1",
            "ServiceAdvisor");

        Assert.Equal(JobStatusService.Completed, result.CurrentStatus);
        Assert.Equal(JobStatusService.Completed, (await db.JobCards.SingleAsync()).Status);
        Assert.Single(await db.JobStatusHistories.ToListAsync());
        Assert.Single(await db.ServiceHistories.ToListAsync());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(
            job.Id,
            JobStatusService.Completed,
            "advisor-1",
            "ServiceAdvisor"));
        Assert.Single(await db.ServiceHistories.ToListAsync());
    }

    [Fact]
    public async Task UpdateStatusAsync_CaptureFailureDoesNotPersistCompletion()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = CreateOptions(databaseName);
        int jobId;

        await using (var setup = new JobMaintenanceDbContext(options))
        {
            var job = CreateJob(JobStatusService.InProgress);
            setup.Add(job);
            await setup.SaveChangesAsync();
            jobId = job.Id;
        }

        await using (var failingContext = new JobMaintenanceDbContext(options))
        {
            var service = CreateJobStatusService(failingContext, new FailingServiceHistoryService());
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(
                jobId,
                JobStatusService.Completed,
                "advisor-1",
                "ServiceAdvisor"));
        }

        await using var verification = new JobMaintenanceDbContext(options);
        Assert.Equal(JobStatusService.InProgress, (await verification.JobCards.SingleAsync()).Status);
        Assert.False(await verification.JobStatusHistories.AnyAsync());
        Assert.False(await verification.ServiceHistories.AnyAsync());
    }

    [Fact]
    public async Task ReadyForCollection_DoesNotAlterCompletedHistory()
    {
        await using var db = CreateDbContext();
        var job = CreateJob(JobStatusService.InProgress);
        db.Add(job);
        await db.SaveChangesAsync();

        var service = CreateJobStatusService(db, new ServiceHistoryService(db));
        await service.UpdateStatusAsync(job.Id, JobStatusService.Completed, "advisor-1", "ServiceAdvisor");
        var original = await db.ServiceHistories.AsNoTracking().SingleAsync();

        await service.UpdateStatusAsync(job.Id, JobStatusService.ReadyForCollection, "advisor-1", "ServiceAdvisor");
        var unchanged = await db.ServiceHistories.AsNoTracking().SingleAsync();

        Assert.Equal(JobStatusService.Completed, unchanged.FinalStatus);
        Assert.Equal(original.CompletedAt, unchanged.CompletedAt);
        Assert.Equal(original.CompletedBy, unchanged.CompletedBy);
    }

    private static JobMaintenanceDbContext CreateDbContext() =>
        new(CreateOptions(Guid.NewGuid().ToString()));

    private static DbContextOptions<JobMaintenanceDbContext> CreateOptions(string databaseName) =>
        new DbContextOptionsBuilder<JobMaintenanceDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    private static JobStatusService CreateJobStatusService(
        JobMaintenanceDbContext db,
        IServiceHistoryService serviceHistory) =>
        new(
            db,
            serviceHistory,
            new ConfigurationBuilder().Build(),
            NullLogger<JobStatusService>.Instance);

    private static JobCard CreateJob(string status) => new()
    {
        JobCardNumber = $"JC-{Guid.NewGuid():N}"[..19].ToUpperInvariant(),
        CheckInId = Random.Shared.Next(1, int.MaxValue),
        CustomerId = 31,
        RecipientUserId = 91,
        VehicleId = 42,
        VehicleRegistrationNumber = "CAB-1234",
        ReportedProblems = "Braking performance requires inspection.",
        Status = status
    };

    private sealed class FailingServiceHistoryService : IServiceHistoryService
    {
        public Task<bool> CaptureCompletedAsync(
            JobCard job,
            string completedBy,
            DateTime completedAt,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated history capture failure.");
    }
}
