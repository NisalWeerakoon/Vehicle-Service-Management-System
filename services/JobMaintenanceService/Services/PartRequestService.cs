using JobMaintenanceService.Data;
using JobMaintenanceService.DTOs;
using JobMaintenanceService.Events;
using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Services;

public interface IPartRequestService
{
    Task<PartRequestResponseDto> CreateAsync(CreatePartRequestDto dto, string mechanicId, string mechanicName, CancellationToken ct = default);
    Task<List<PartRequestResponseDto>> GetMineAsync(string mechanicId, CancellationToken ct = default);
}

public class PartRequestService(JobMaintenanceDbContext db, IPartRequestEventPublisher publisher) : IPartRequestService
{
    public async Task<PartRequestResponseDto> CreateAsync(CreatePartRequestDto dto, string mechanicId, string mechanicName, CancellationToken ct = default)
    {
        var job = await db.JobCards.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.JobCardId, ct) ?? throw new KeyNotFoundException("Job card not found.");
        var assigned = await db.MechanicAssignments.AnyAsync(x => x.JobCardId == dto.JobCardId && x.MechanicId == mechanicId && x.IsActive, ct);
        if (!assigned) throw new UnauthorizedAccessException("You are not assigned to this job card.");
        var request = new PartRequest { JobCardId = job.Id, SparePartId = dto.SparePartId, RequestedQuantity = dto.RequestedQuantity, RequestingMechanicId = mechanicId, RequestingMechanicName = mechanicName, RequestedAt = DateTime.UtcNow };
        db.PartRequests.Add(request);
        await db.SaveChangesAsync(ct);
        var evt = new PartRequestedEvent { CorrelationId = request.Id.ToString(), Data = new PartRequestedData { RequestId = request.Id, JobCardId = job.Id, JobCardNumber = job.JobCardNumber, SparePartId = request.SparePartId, RequestedQuantity = request.RequestedQuantity, RequestingMechanicId = mechanicId, RequestingMechanicName = mechanicName } };
        await publisher.PublishAsync(evt, ct);
        return ToResponse(request, job.JobCardNumber);
    }

    public async Task<List<PartRequestResponseDto>> GetMineAsync(string mechanicId, CancellationToken ct = default) =>
        await db.PartRequests.AsNoTracking().Where(x => x.RequestingMechanicId == mechanicId).OrderByDescending(x => x.RequestedAt)
            .Join(db.JobCards.AsNoTracking(), x => x.JobCardId, j => j.Id, (x, j) => ToResponse(x, j.JobCardNumber)).ToListAsync(ct);

    private static PartRequestResponseDto ToResponse(PartRequest x, string number) => new() { Id = x.Id, JobCardId = x.JobCardId, JobCardNumber = number, SparePartId = x.SparePartId, RequestedQuantity = x.RequestedQuantity, RequestingMechanicId = x.RequestingMechanicId, RequestingMechanicName = x.RequestingMechanicName, RequestedAt = x.RequestedAt };
}
