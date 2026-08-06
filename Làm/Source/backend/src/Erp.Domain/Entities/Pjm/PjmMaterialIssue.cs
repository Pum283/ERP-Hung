using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Xuất NVL cho dự án — soft local (UC_PJM_021).</summary>
public class PjmMaterialIssue : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = "";
    /// <summary>Draft | Posted</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public ICollection<PjmMaterialIssueLine> Lines { get; set; } = new List<PjmMaterialIssueLine>();
}

public class PjmMaterialIssueLine : TenantEntity
{
    public Guid MaterialIssueId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "CAI";
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
}
