using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Lịch sử tách bill / gộp hóa đơn POS (UC_POS_028).</summary>
public class PosOrderSplitMergeHistory : TenantEntity
{
    public Guid SourceOrderId { get; set; }
    public Guid TargetOrderId { get; set; }
    /// <summary>Split | Merge</summary>
    public string OperationType { get; set; } = "Split";
    public string ItemDetailsJson { get; set; } = "[]";
    public Guid PerformedByUserId { get; set; }
    public string Reason { get; set; } = "";
    public DateTimeOffset OperationTime { get; set; } = DateTimeOffset.UtcNow;
}
