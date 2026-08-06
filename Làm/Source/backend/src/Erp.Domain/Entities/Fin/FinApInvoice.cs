using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Hóa đơn phải trả AP (UC_FIN_039–040).</summary>
public class FinApInvoice : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid VendorId { get; set; }
    public string? VendorInvoiceNo { get; set; }
    public Guid? PurVendorInvoiceId { get; set; }
    public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    /// <summary>Draft · Open · Partial · Paid · Void</summary>
    public string Status { get; set; } = "Draft";
    public Guid? PeriodId { get; set; }
    public Guid? ApAccountId { get; set; }
    public Guid? ExpenseAccountId { get; set; }
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}

/// <summary>Đề nghị thanh toán AP (UC_FIN_041–042).</summary>
public class FinApPaymentRequest : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid VendorId { get; set; }
    public DateTimeOffset RequestDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal RequestAmount { get; set; }
    /// <summary>Cash · Bank</summary>
    public string PayMethod { get; set; } = "Bank";
    public Guid? CashFundId { get; set; }
    public Guid? BankAccountId { get; set; }
    /// <summary>Draft · Submitted · Approved · Rejected · Paid · Void</summary>
    public string Status { get; set; } = "Draft";
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? PaymentId { get; set; }
    public string? PaymentCode { get; set; }
    public string? Note { get; set; }
}

public class FinApPaymentRequestLine : TenantEntity
{
    public Guid PaymentRequestId { get; set; }
    public Guid ApInvoiceId { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>Thanh toán AP & phân bổ (UC_FIN_043).</summary>
public class FinApPayment : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid VendorId { get; set; }
    public DateTimeOffset PayDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    /// <summary>Cash · Bank</summary>
    public string PayMethod { get; set; } = "Bank";
    public Guid? CashFundId { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? PaymentRequestId { get; set; }
    public Guid? CashVoucherId { get; set; }
    public Guid? BankVoucherId { get; set; }
    /// <summary>Draft · Posted · Void</summary>
    public string Status { get; set; } = "Draft";
    public Guid? PeriodId { get; set; }
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}

public class FinApPaymentAllocation : TenantEntity
{
    public Guid PaymentId { get; set; }
    public Guid ApInvoiceId { get; set; }
    public decimal Amount { get; set; }
}
