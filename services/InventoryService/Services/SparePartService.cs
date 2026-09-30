using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services;

public interface ISparePartService
{
    Task<List<SparePartResponseDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default);
    Task<SparePartResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SparePartResponseDto> CreateAsync(CreateSparePartDto dto, CancellationToken cancellationToken = default);
    Task<SparePartResponseDto> UpdateAsync(int id, UpdateSparePartDto dto, CancellationToken cancellationToken = default);
    Task<SparePartResponseDto> AdjustStockAsync(int id, AdjustStockDto dto, CancellationToken cancellationToken = default);
    Task<List<StockReportItemDto>> GetCurrentStockReportAsync(CancellationToken cancellationToken = default);
    Task<List<StockReportItemDto>> GetLowStockReportAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class SparePartService : ISparePartService
{
    private readonly InventoryDbContext _db;
    private readonly ILowStockEventPublisher _lowStockEvents;

    public SparePartService(InventoryDbContext db, ILowStockEventPublisher lowStockEvents)
    {
        _db = db;
        _lowStockEvents = lowStockEvents;
    }

    public async Task<List<SparePartResponseDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = _db.SpareParts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Id.ToString().Contains(term));
        }

        return await query.OrderBy(x => x.Name).Select(ToResponse()).ToListAsync(cancellationToken);
    }

    public async Task<SparePartResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.SpareParts.AsNoTracking().Where(x => x.Id == id).Select(ToResponse()).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Spare part not found.");

    public async Task<SparePartResponseDto> CreateAsync(CreateSparePartDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto);
        var part = new SparePart
        {
            Name = dto.Name.Trim(), Description = dto.Description.Trim(), Quantity = dto.Quantity,
            LowStockThreshold = dto.LowStockThreshold, UnitPrice = dto.UnitPrice, CreatedAt = DateTime.UtcNow
        };
        _db.SpareParts.Add(part);
        await _db.SaveChangesAsync(cancellationToken);
        await _lowStockEvents.PublishIfTransitionedToLowStockAsync(part, false, cancellationToken);
        return await GetByIdAsync(part.Id, cancellationToken);
    }

    public async Task<SparePartResponseDto> UpdateAsync(int id, UpdateSparePartDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto);
        var part = await FindAsync(id, cancellationToken);
        var wasLowStock = IsLowStock(part);
        part.Name = dto.Name.Trim(); part.Description = dto.Description.Trim(); part.Quantity = dto.Quantity;
        part.LowStockThreshold = dto.LowStockThreshold; part.UnitPrice = dto.UnitPrice; part.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _lowStockEvents.PublishIfTransitionedToLowStockAsync(part, wasLowStock, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<SparePartResponseDto> AdjustStockAsync(int id, AdjustStockDto dto, CancellationToken cancellationToken = default)
    {
        var part = await FindAsync(id, cancellationToken);
        if ((long)part.Quantity + dto.Adjustment < 0)
            throw new InvalidOperationException("Stock adjustment would result in a negative quantity.");

        var wasLowStock = IsLowStock(part);
        part.Quantity += dto.Adjustment;
        part.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _lowStockEvents.PublishIfTransitionedToLowStockAsync(part, wasLowStock, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public Task<List<StockReportItemDto>> GetCurrentStockReportAsync(CancellationToken cancellationToken = default) =>
        StockReportQuery().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<List<StockReportItemDto>> GetLowStockReportAsync(CancellationToken cancellationToken = default) =>
        StockReportQuery().Where(x => x.IsLowStock).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var part = await FindAsync(id, cancellationToken);
        _db.SpareParts.Remove(part);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<SparePart> FindAsync(int id, CancellationToken cancellationToken) =>
        await _db.SpareParts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Spare part not found.");

    private static System.Linq.Expressions.Expression<Func<SparePart, SparePartResponseDto>> ToResponse() => x => new SparePartResponseDto
    {
        Id = x.Id, Name = x.Name, Description = x.Description, Quantity = x.Quantity,
        LowStockThreshold = x.LowStockThreshold, IsLowStock = x.Quantity <= x.LowStockThreshold,
        UnitPrice = x.UnitPrice, CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private IQueryable<StockReportItemDto> StockReportQuery() => _db.SpareParts.AsNoTracking().Select(x => new StockReportItemDto
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        CurrentQuantity = x.Quantity,
        LowStockThreshold = x.LowStockThreshold,
        UnitPrice = x.UnitPrice,
        IsLowStock = x.Quantity <= x.LowStockThreshold
    });

    private static bool IsLowStock(SparePart part) => part.Quantity <= part.LowStockThreshold;

    private static void Validate(CreateSparePartDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length < 2)
            throw new ArgumentException("Name is required and must contain at least 2 characters.");
        if (dto.Name.Trim().Length > 200) throw new ArgumentException("Name cannot exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Trim().Length < 2)
            throw new ArgumentException("Description is required and must contain at least 2 characters.");
        if (dto.Description.Trim().Length > 1000) throw new ArgumentException("Description cannot exceed 1000 characters.");
        if (dto.Quantity < 0) throw new ArgumentException("Quantity cannot be negative.");
        if (dto.LowStockThreshold < 0) throw new ArgumentException("Low-stock threshold cannot be negative.");
        if (dto.UnitPrice < 0) throw new ArgumentException("Unit price cannot be negative.");
    }
}
