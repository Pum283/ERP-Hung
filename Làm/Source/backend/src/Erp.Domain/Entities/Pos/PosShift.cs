using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Ca thu ngân (UC_POS_042–048).</summary>
public class PosShift : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid StoreId { get; set; }
    public Guid? TerminalId { get; set; }
    public Guid CashierUserId { get; set; }
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCashCounted { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    /// <summary>Open · Closed</summary>
    public string Status { get; set; } = "Open";
    public DateTimeOffset? ReportPrintedAt { get; set; }
    public string? Note { get; set; }
}
