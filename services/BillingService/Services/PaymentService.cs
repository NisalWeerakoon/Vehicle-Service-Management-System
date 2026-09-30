using System.Data;
using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public interface IPaymentService
{
    Task<InvoiceResponseDto> RecordAsync(int invoiceId, RecordPaymentDto dto, string recordedBy, CancellationToken ct = default);
}

public class PaymentService(BillingDbContext db, IInvoiceEventPublisher eventPublisher) : IPaymentService
{
    public async Task<InvoiceResponseDto> RecordAsync(int invoiceId, RecordPaymentDto dto, string recordedBy, CancellationToken ct = default)
    {
        if (invoiceId <= 0 || dto.Amount <= 0 || string.IsNullOrWhiteSpace(dto.ReferenceNumber))
            throw new InvalidOperationException("A positive payment amount and reference number are required.");
        var paymentDate = dto.PaymentDate?.ToUniversalTime() ?? DateTime.UtcNow;
        if (paymentDate > DateTime.UtcNow.AddMinutes(5))
            throw new InvalidOperationException("Payment date cannot be in the future.");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var invoice = await db.Invoices.Include(x => x.ChargeLines).Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.Id == invoiceId, ct)
            ?? throw new KeyNotFoundException("Invoice not found.");
        if (!invoice.IsGenerated)
            throw new InvalidOperationException("Payment can only be recorded against a generated invoice.");
        if (invoice.PaymentStatus == PaymentStatus.Paid || invoice.AmountPaid >= invoice.TotalAmount)
            throw new InvalidOperationException("Payment cannot be recorded because this invoice has already been paid.");
        if (dto.Amount > invoice.TotalAmount - invoice.AmountPaid)
            throw new InvalidOperationException("Payment amount cannot exceed the remaining invoice balance.");

        var reference = dto.ReferenceNumber.Trim();
        if (await db.Payments.AnyAsync(x => x.ReferenceNumber == reference, ct))
            throw new InvalidOperationException("A payment with this reference number has already been recorded.");

        var payment = new Payment { InvoiceId = invoice.Id, Amount = dto.Amount, PaymentDate = paymentDate, ReferenceNumber = reference, CreatedBy = string.IsNullOrWhiteSpace(recordedBy) ? "Accounts" : recordedBy.Trim() };
        invoice.Payments.Add(payment);
        invoice.AmountPaid += payment.Amount;
        invoice.PaymentStatus = invoice.AmountPaid == invoice.TotalAmount ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await eventPublisher.PublishPaymentAsync(payment, invoice, ct);
        return InvoiceService.Map(invoice);
    }
}
