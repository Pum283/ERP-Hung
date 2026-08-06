using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Kế hoạch SX theo đơn hàng (UC_MFG_013).</summary>
public class MfgPlan : TenantEntity
{
    public string Code { get; set; } = "";
    public string SourceOrderCode { get; set; } = "";
    /// <summary>Draft · Confirmed · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
    public Guid CreatedByUserId { get; set; }
}
