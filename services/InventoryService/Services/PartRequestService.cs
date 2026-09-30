using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Models;
using InventoryService.Events;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services;

public interface IPartRequestService
{
    Task<PartRequestResponseDto> CreateAsync(CreatePartRequestDto dto, string mechanicId, string mechanicName, string bearerToken, CancellationToken cancellationToken = default);
    Task<List<PartRequestResponseDto>> GetPendingAsync(string bearerToken, CancellationToken cancellationToken = default);
    Task<List<PartRequestResponseDto>> GetByMechanicAsync(string mechanicId, string bearerToken, CancellationToken cancellationToken = default);
    Task<PartRequestResponseDto> GetByIdAsync(int id, string bearerToken, CancellationToken cancellationToken = default);
    Task<PartRequestResponseDto> IssueAsync(int id, string officerId, string officerName, string bearerToken, CancellationToken cancellationToken = default);
}

public class PartRequestService : IPartRequestService
{
    private readonly InventoryDbContext _db;
    private readonly IJobCardGateway _jobs;
    private readonly ILowStockEventPublisher _lowStockEvents;
    private readonly IPartIssuedEventPublisher _partIssuedEvents;
    public PartRequestService(InventoryDbContext db, IJobCardGateway jobs, ILowStockEventPublisher lowStockEvents, IPartIssuedEventPublisher partIssuedEvents)
    {
        _db = db;
        _jobs = jobs;
        _lowStockEvents = lowStockEvents;
        _partIssuedEvents = partIssuedEvents;
    }

    public async Task<PartRequestResponseDto> CreateAsync(CreatePartRequestDto dto, string mechanicId, string mechanicName, string bearerToken, CancellationToken cancellationToken = default)
    {
        var job = await _jobs.GetJobAsync(dto.JobCardId, bearerToken, cancellationToken);
        await _jobs.EnsureMechanicIsAssignedAsync(dto.JobCardId, mechanicId, bearerToken, cancellationToken);
        var part = await _db.SpareParts.FirstOrDefaultAsync(x => x.Id == dto.SparePartId, cancellationToken)
            ?? throw new KeyNotFoundException("Spare part not found.");
        var request = new PartRequest { JobCardId = job.Id, JobCardNumber = job.JobCardNumber, SparePartId = part.Id, RequestedQuantity = dto.RequestedQuantity, RequestingMechanicId = mechanicId, RequestingMechanicName = mechanicName, Status = PartRequestStatus.Pending, RequestedAt = DateTime.UtcNow };
        _db.PartRequests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(request, part, job.JobCardNumber);
    }

    public Task<List<PartRequestResponseDto>> GetPendingAsync(string bearerToken, CancellationToken cancellationToken = default) =>
        GetListAsync(_db.PartRequests.Where(x => x.Status == PartRequestStatus.Pending), bearerToken, cancellationToken);

    public Task<List<PartRequestResponseDto>> GetByMechanicAsync(string mechanicId, string bearerToken, CancellationToken cancellationToken = default) =>
        GetListAsync(_db.PartRequests.Where(x => x.RequestingMechanicId == mechanicId), bearerToken, cancellationToken);

    public async Task<PartRequestResponseDto> GetByIdAsync(int id, string bearerToken, CancellationToken cancellationToken = default)
    {
        var request = await RequestQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new KeyNotFoundException("Part request not found.");
        return await ToResponseAsync(request, bearerToken, cancellationToken);
    }

    public async Task<PartRequestResponseDto> IssueAsync(int id, string officerId, string officerName, string bearerToken, CancellationToken cancellationToken = default)
    {
        var snapshot = await RequestQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new KeyNotFoundException("Part request not found.");
        if (snapshot.Status != PartRequestStatus.Pending) throw new InvalidOperationException("This part request has already been issued.");
        var wasLowStock = snapshot.SparePart!.Quantity <= snapshot.SparePart.LowStockThreshold;
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var issuedAt = DateTime.UtcNow;
        var requestChanged = await _db.Database.ExecuteSqlInterpolatedAsync($@"UPDATE PartRequests SET Status = {PartRequestStatus.Issued}, IssuedAt = {issuedAt} WHERE Id = {id} AND Status = {PartRequestStatus.Pending}", cancellationToken);
        if (requestChanged != 1) throw new InvalidOperationException("This part request has already been issued.");
        var stockChanged = await _db.Database.ExecuteSqlInterpolatedAsync($@"UPDATE SpareParts SET Quantity = Quantity - {snapshot.RequestedQuantity}, UpdatedAt = {issuedAt} WHERE Id = {snapshot.SparePartId} AND Quantity >= {snapshot.RequestedQuantity}", cancellationToken);
        if (stockChanged != 1) throw new InvalidOperationException("Insufficient stock to issue the requested quantity.");
        var issue = new PartIssue { PartRequestId = snapshot.Id, JobCardId = snapshot.JobCardId, SparePartId = snapshot.SparePartId, QuantityIssued = snapshot.RequestedQuantity, UnitPrice = snapshot.SparePart.UnitPrice, TotalAmount = snapshot.SparePart.UnitPrice * snapshot.RequestedQuantity, InventoryOfficerId = officerId, InventoryOfficerName = officerName, IssuedAt = issuedAt };
        _db.PartIssues.Add(issue);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        var updatedPart = await _db.SpareParts.AsNoTracking().FirstAsync(x => x.Id == snapshot.SparePartId, cancellationToken);
        await _lowStockEvents.PublishIfTransitionedToLowStockAsync(updatedPart, wasLowStock, cancellationToken);
        await _partIssuedEvents.PublishAsync(new PartIssuedEvent { CorrelationId = snapshot.SourceRequestId?.ToString() ?? snapshot.Id.ToString(), Data = new PartIssuedData { IssueId = issue.Id, RequestId = snapshot.SourceRequestId ?? snapshot.Id, JobCardId = snapshot.JobCardId, SparePartId = snapshot.SparePartId, SparePartName = snapshot.SparePart.Name, QuantityIssued = issue.QuantityIssued, UnitPrice = issue.UnitPrice, TotalAmount = issue.TotalAmount } }, cancellationToken);
        return await GetByIdAsync(id, bearerToken, cancellationToken);
    }

    private async Task<List<PartRequestResponseDto>> GetListAsync(IQueryable<PartRequest> query, string bearerToken, CancellationToken cancellationToken)
    {
        var requests = await query.Include(x => x.SparePart).Include(x => x.PartIssue).AsNoTracking().OrderByDescending(x => x.RequestedAt).ToListAsync(cancellationToken);
        var result = new List<PartRequestResponseDto>();
        foreach (var request in requests) result.Add(await ToResponseAsync(request, bearerToken, cancellationToken));
        return result;
    }

    private IQueryable<PartRequest> RequestQuery() => _db.PartRequests.Include(x => x.SparePart).Include(x => x.PartIssue);
    private Task<PartRequestResponseDto> ToResponseAsync(PartRequest request, string token, CancellationToken ct) => Task.FromResult(ToResponse(request, request.SparePart!, request.JobCardNumber));
    private static PartRequestResponseDto ToResponse(PartRequest request, SparePart part, string jobCardNumber) => new()
    {
        Id = request.Id, JobCardId = request.JobCardId, JobCardNumber = jobCardNumber, SparePartId = request.SparePartId, SparePartName = part.Name, RequestedQuantity = request.RequestedQuantity, CurrentStock = part.Quantity, RequestingMechanicId = request.RequestingMechanicId, RequestingMechanicName = request.RequestingMechanicName, Status = request.Status, RequestedAt = request.RequestedAt, IssuedAt = request.IssuedAt,
        Issue = request.PartIssue is null ? null : new PartIssueResponseDto { Id = request.PartIssue.Id, PartRequestId = request.PartIssue.PartRequestId, JobCardId = request.PartIssue.JobCardId, SparePartId = request.PartIssue.SparePartId, QuantityIssued = request.PartIssue.QuantityIssued, InventoryOfficerId = request.PartIssue.InventoryOfficerId, InventoryOfficerName = request.PartIssue.InventoryOfficerName, IssuedAt = request.PartIssue.IssuedAt }
    };
}
