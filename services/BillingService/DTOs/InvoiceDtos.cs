using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class AddManualChargeDto
{
    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.0001", "999999999")] public decimal Quantity { get; set; }
    [Range(typeof(decimal), "0", "999999999")] public decimal UnitPrice { get; set; }
}

public class InvoiceResponseDto
{
    public int Id { get; set; }
    public string? InvoiceNumber { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public bool IsBillingEligible { get; set; }
    public bool IsGenerated { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ChargeLineResponseDto> ChargeLines { get; set; } = [];
    public List<PaymentResponseDto> Payments { get; set; } = [];
}

public class RecordPaymentDto
{
    [Range(typeof(decimal), "0.01", "999999999")] public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    [Required, MaxLength(100)] public string ReferenceNumber { get; set; } = string.Empty;
}

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}

public class ChargeLineResponseDto
{
    public int Id { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class InvoicePaymentReportDto
{
    public DateTime GeneratedAt { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int PaidCount { get; set; }
    public int PartiallyPaidCount { get; set; }
    public int UnpaidCount { get; set; }
    public List<InvoicePaymentReportItemDto> Items { get; set; } = [];
}

public class InvoicePaymentReportItemDto
{
    public int InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int JobCardId { get; set; }
    public string JobCardNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public int PaymentCount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}
