using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Xuất NVL cho lệnh (UC_MFG_020).</summary>
public class MfgMaterialIssue : TenantEntity
{
    public Guid WorkOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public string Unit { get; set; } = "CAI";
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid IssuedBy { get; set; }
    public string? Note { get; set; }
}
