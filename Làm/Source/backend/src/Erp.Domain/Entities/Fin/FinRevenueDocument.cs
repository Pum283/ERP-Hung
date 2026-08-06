using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Ghi nhận doanh thu / giá vốn (UC_FIN_057–058, 060).</summary>
public class FinRevenueDocument : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>PosRevenue · OrderRevenue · ArRevenue · Cogs</summary>
    public string Kind { get; set; } = "PosRevenue";
    public string SourceModule { get; set; } = "";
    public Guid? SourceId { get; set; }
    public string? SourceCode { get; set; }
    public DateTimeOffset DocDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal RevenueAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal CogsAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? PeriodId { get; set; }
    public Guid? DebitAccountId { get; set; }
    public Guid? CreditAccountId { get; set; }
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    /// <summary>Draft · Posted · Void</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}
