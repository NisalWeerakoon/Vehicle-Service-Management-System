using JobMaintenanceService.Data;
using JobMaintenanceService.Models;
using JobMaintenanceService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace JobMaintenanceService.Tests;

public class ServiceHistoryQueryTests
{
    [Fact]
    public async Task GetByVehicleAsync_ReturnsNewestFirstTypedDetails()
    {
        await using var db = CreateDb();
        db.ServiceHistories.AddRange(
            History(1, 20, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)),
            History(2, 20, new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        var result = await Query(db).GetByVehicleAsync(10, null);

        Assert.Equal([2, 1], result.Records.Select(x => x.Id));
        Assert.Equal("Replace brake pads", result.Records[0].CompletedRepairTasks.Single().Title);
        Assert.Equal("Road test completed.", result.Records[0].RepairNotes.Single().Note);
        Assert.Equal("Brake pads worn.", result.Records[0].Inspection?.InspectionResults);
    }

    [Fact]
    public async Task GetByVehicleAsync_HandlesEmptyFilteredAndInvalidJsonResults()
    {
        await using var db = CreateDb();
        var history = History(1, 20, DateTime.UtcNow);
        history.CompletedRepairTasksJson = "not-json";
        history.RepairNotesJson = "{";
        db.Add(history);
        await db.SaveChangesAsync();

        Assert.Empty((await Query(db).GetByVehicleAsync(999, null)).Records);
        Assert.Empty((await Query(db).GetByVehicleAsync(10, 21)).Records);
        var owned = (await Query(db).GetByVehicleAsync(10, 20)).Records.Single();
        Assert.Empty(owned.CompletedRepairTasks);
        Assert.Empty(owned.RepairNotes);
    }

    private static ServiceHistoryQueryService Query(JobMaintenanceDbContext db) =>
        new(db, NullLogger<ServiceHistoryQueryService>.Instance);

    private static JobMaintenanceDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<JobMaintenanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ServiceHistory History(int id, int customerId, DateTime completedAt) => new()
    {
        Id = id,
        JobCardId = id,
        JobCardNumber = $"JC-{id:000}",
        VehicleId = 10,
        VehicleRegistrationNumber = "CAB-1234",
        CustomerId = customerId,
        ReportedProblems = "Brake inspection required.",
        CompletedBy = "advisor-1",
        FinalStatus = "Completed",
        CompletedAt = completedAt,
        CapturedAt = completedAt,
        CompletedRepairTasksJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { id = 1, title = "Replace brake pads", description = "Pads replaced.", completedAt = (DateTime?)null }
        }),
        RepairNotesJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { id = 1, note = "Road test completed.", createdAt = completedAt, updatedAt = (DateTime?)null }
        }),
        InspectionResults = "Brake pads worn.",
        IdentifiedProblems = "Reduced braking performance."
    };
}
