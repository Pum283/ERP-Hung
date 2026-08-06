using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Hóa đơn phải thu AR (UC_FIN_030–031).</summary>
public class FinArInvoice : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid CustomerId { get; set; }
    public string? CustomerInvoiceNo { get; set; }
    public Guid? CrmOrderId { get; set; }
    public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    /// <summary>Draft · Open · Partial · Paid · Void</summary>
    public string Status { get; set; } = "Draft";
    public bool CreditLimitWarned { get; set; }
    public Guid? PeriodId { get; set; }
    public Guid? ArAccountId { get; set; }
    public Guid? RevenueAccountId { get; set; }
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}

/// <summary>Hạn mức tín dụng khách (UC_FIN_035).</summary>
public class FinArCreditLimit : TenantEntity
{
    public Guid CustomerId { get; set; }
    public decimal CreditLimit { get; set; }
    /// <summary>% cảnh báo sớm (vd 80 = cảnh báo khi đạt 80% hạn mức).</summary>
    public decimal WarningPercent { get; set; } = 80;
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}

/// <summary>Phiếu thu AR & phân bổ (UC_FIN_032).</summary>
public class FinArReceipt : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid CustomerId { get; set; }
    public DateTimeOffset ReceiptDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    /// <summary>Cash · Bank</summary>
    public string PayMethod { get; set; } = "Bank";
    public Guid? CashFundId { get; set; }
    public Guid? BankAccountId { get; set; }
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

public class FinArReceiptAllocation : TenantEntity
{
    public Guid ReceiptId { get; set; }
    public Guid ArInvoiceId { get; set; }
    public decimal Amount { get; set; }
}
