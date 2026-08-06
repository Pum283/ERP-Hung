using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Trả hàng / hoàn tiền (UC_POS_040).</summary>
public class PosReturn : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid SaleId { get; set; }
    public Guid? ShiftId { get; set; }
    /// <summary>Draft · Completed</summary>
    public string Status { get; set; } = "Draft";
    public decimal RefundAmount { get; set; }
    /// <summary>Cash · Transfer · Card · Wallet</summary>
    public string RefundMethod { get; set; } = "Cash";
    public string? Reason { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
