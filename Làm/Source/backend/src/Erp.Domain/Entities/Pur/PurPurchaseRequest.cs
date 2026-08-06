using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Yêu cầu mua hàng PR (UC_PUR_014, 017–019).</summary>
public class PurPurchaseRequest : TenantEntity
{
    public string Code { get; set; } = "";
    public string? RequestingUnit { get; set; }
    public string? Note { get; set; }
    /// <summary>Draft · Submitted · Approved · Rejected · Returned</summary>
    public string Status { get; set; } = "Draft";
    public string? DecisionNote { get; set; }
    public Guid RequestedBy { get; set; }
    public Guid? DecidedBy { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}
