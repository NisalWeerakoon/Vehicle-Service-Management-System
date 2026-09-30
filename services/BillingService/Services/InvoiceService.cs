using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> GetByJobAsync(int jobCardId, CancellationToken ct = default);
    Task<InvoiceResponseDto> AddManualAsync(int jobCardId, ChargeType type, AddManualChargeDto dto, CancellationToken ct = default);
    Task AddPartAsync(int jobCardId, int issueId, int requestId, int sparePartId, string description, decimal quantity, decimal unitPrice, CancellationToken ct = default);
    Task<InvoiceResponseDto> GenerateAsync(int jobCardId, CancellationToken ct = default);
    Task<InvoiceResponseDto?> GetByIdAsync(int invoiceId, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceResponseDto>> GetEligibleAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceResponseDto>> GetGeneratedAsync(int? customerId, CancellationToken ct = default);
}

public class InvoiceService(BillingDbContext db, IInvoiceEventPublisher eventPublisher) : IInvoiceService
{
    public async Task<InvoiceResponseDto> GetByJobAsync(int jobCardId, CancellationToken ct = default)
    {
        var invoice = await IncludeLines().AsNoTracking().SingleOrDefaultAsync(x => x.JobCardId == jobCardId, ct);
        return invoice is null ? new InvoiceResponseDto { JobCardId = jobCardId } : Map(invoice);
    }

    public async Task<InvoiceResponseDto?> GetByIdAsync(int invoiceId, CancellationToken ct = default)
    {
        var invoice = await IncludeLines().AsNoTracking().SingleOrDefaultAsync(x => x.Id == invoiceId && x.IsGenerated, ct);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<IReadOnlyList<InvoiceResponseDto>> GetEligibleAsync(CancellationToken ct = default) =>
        (await IncludeLines().AsNoTracking().Where(x => x.IsBillingEligible && !x.IsGenerated)
            .OrderBy(x => x.CreatedAt).ToListAsync(ct)).Select(Map).ToList();

    public async Task<IReadOnlyList<InvoiceResponseDto>> GetGeneratedAsync(int? customerId, CancellationToken ct = default)
    {
        var query = IncludeLines().AsNoTracking().Where(x => x.IsGenerated);
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId.Value);
        return (await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<InvoiceResponseDto> AddManualAsync(int jobCardId, ChargeType type, AddManualChargeDto dto, CancellationToken ct = default)
    {
        if (type is not (ChargeType.Service or ChargeType.Labour) || string.IsNullOrWhiteSpace(dto.Description) || dto.Quantity <= 0 || dto.UnitPrice < 0)
            throw new InvalidOperationException("Description is required, quantity must be greater than zero, and unit price cannot be negative.");

        var invoice = await GetEligibleDraftAsync(jobCardId, ct);
        invoice.ChargeLines.Add(NewLine(type, dto.Description, dto.Quantity, dto.UnitPrice, null));
        await RecalculateAndSaveAsync(invoice, ct);
        return Map(invoice);
    }

    public async Task AddPartAsync(int jobCardId, int issueId, int requestId, int sparePartId, string description, decimal quantity, decimal unitPrice, CancellationToken ct = default)
    {
        if (jobCardId <= 0 || issueId <= 0 || requestId <= 0 || sparePartId <= 0 || quantity <= 0 || unitPrice < 0 || string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Invalid issued-part charge data.");
        if (await db.PartCharges.AnyAsync(x => x.PartIssueId == issueId, ct)) return;
        db.PartCharges.Add(new PartCharge { JobCardId = jobCardId, PartIssueId = issueId, PartRequestId = requestId, SparePartId = sparePartId, SparePartName = description.Trim(), Quantity = checked((int)quantity), UnitPrice = unitPrice, TotalAmount = quantity * unitPrice });
        await db.SaveChangesAsync(ct);
    }

    public async Task<InvoiceResponseDto> GenerateAsync(int jobCardId, CancellationToken ct = default)
    {
        var invoice = await IncludeLines().SingleOrDefaultAsync(x => x.JobCardId == jobCardId, ct)
            ?? throw new InvalidOperationException("Invoice cannot be generated because the service has not been completed.");
        if (!invoice.IsBillingEligible)
            throw new InvalidOperationException("Invoice cannot be generated because the service has not been completed.");
        if (invoice.IsGenerated)
            throw new InvalidOperationException("An invoice has already been generated for this job.");
        if (invoice.CustomerId <= 0 || invoice.VehicleId <= 0 || string.IsNullOrWhiteSpace(invoice.VehicleRegistrationNumber))
            throw new InvalidOperationException("Invoice cannot be generated because required customer or vehicle information is missing.");

        var partCharges = await db.PartCharges.Where(x => x.JobCardId == jobCardId).ToListAsync(ct);
        foreach (var part in partCharges.Where(p => !invoice.ChargeLines.Any(l => l.SourceReferenceId == p.PartIssueId.ToString())))
            invoice.ChargeLines.Add(NewLine(ChargeType.Part, part.SparePartName, part.Quantity, part.UnitPrice, part.PartIssueId.ToString()));

        invoice.TotalAmount = invoice.ChargeLines.Sum(x => x.Quantity * x.UnitPrice);
        foreach (var line in invoice.ChargeLines) line.LineTotal = line.Quantity * line.UnitPrice;
        invoice.IsGenerated = true;
        invoice.InvoiceNumber = $"INV-{DateTime.UtcNow:yyyy}-{invoice.Id:D6}";
        invoice.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await eventPublisher.PublishAsync(invoice, ct);
        return Map(invoice);
    }

    private async Task<Invoice> GetEligibleDraftAsync(int jobCardId, CancellationToken ct)
    {
        var invoice = await IncludeLines().SingleOrDefaultAsync(x => x.JobCardId == jobCardId, ct)
            ?? throw new InvalidOperationException("Invoice cannot be generated because the service has not been completed.");
        if (!invoice.IsBillingEligible || invoice.IsGenerated)
            throw new InvalidOperationException(invoice.IsGenerated ? "The invoice has already been generated and cannot be changed." : "Invoice cannot be generated because the service has not been completed.");
        return invoice;
    }

    private IQueryable<Invoice> IncludeLines() => db.Invoices.Include(x => x.ChargeLines).Include(x => x.Payments);
    private static ChargeLine NewLine(ChargeType type, string description, decimal quantity, decimal unitPrice, string? source) => new() { ChargeType = type, Description = description.Trim(), Quantity = quantity, UnitPrice = unitPrice, LineTotal = quantity * unitPrice, SourceReferenceId = source };
    private async Task RecalculateAndSaveAsync(Invoice invoice, CancellationToken ct) { invoice.TotalAmount = invoice.ChargeLines.Sum(x => x.LineTotal); invoice.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
    internal static InvoiceResponseDto Map(Invoice x) => new() { Id = x.Id, InvoiceNumber = x.InvoiceNumber, JobCardId = x.JobCardId, JobCardNumber = x.JobCardNumber, CustomerId = x.CustomerId, VehicleId = x.VehicleId, VehicleRegistrationNumber = x.VehicleRegistrationNumber, IsBillingEligible = x.IsBillingEligible, IsGenerated = x.IsGenerated, TotalAmount = x.TotalAmount, AmountPaid = x.AmountPaid, RemainingBalance = x.TotalAmount - x.AmountPaid, PaymentStatus = x.PaymentStatus.ToString(), CreatedAt = x.CreatedAt, ChargeLines = x.ChargeLines.OrderBy(x => x.CreatedAt).Select(x => new ChargeLineResponseDto { Id = x.Id, ChargeType = x.ChargeType.ToString().ToUpperInvariant(), Description = x.Description, Quantity = x.Quantity, UnitPrice = x.UnitPrice, LineTotal = x.LineTotal }).ToList(), Payments = x.Payments.OrderByDescending(x => x.PaymentDate).Select(x => new PaymentResponseDto { Id = x.Id, InvoiceId = x.InvoiceId, Amount = x.Amount, PaymentDate = x.PaymentDate, ReferenceNumber = x.ReferenceNumber, CreatedBy = x.CreatedBy }).ToList() };
}
