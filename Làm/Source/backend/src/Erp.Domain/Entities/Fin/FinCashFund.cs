using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Quỹ tiền mặt (UC_FIN_018).</summary>
public class FinCashFund : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid CashAccountId { get; set; }
    public Guid? CustodianUserId { get; set; }
    public string? CustodianName { get; set; }
    public decimal OpeningBalance { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

/// <summary>Phiếu thu/chi tiền mặt (UC_FIN_019–020).</summary>
public class FinCashVoucher : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid FundId { get; set; }
    /// <summary>Receipt · Payment</summary>
    public string VoucherType { get; set; } = "Receipt";
    public DateTimeOffset DocDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string? PartnerCode { get; set; }
    public Guid? CounterAccountId { get; set; }
    public Guid? PeriodId { get; set; }
    /// <summary>Draft · Posted · Void</summary>
    public string Status { get; set; } = "Draft";
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}
