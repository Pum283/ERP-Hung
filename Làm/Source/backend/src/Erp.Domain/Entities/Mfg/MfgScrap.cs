using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Phế phẩm / hao hụt lệnh SX (UC_MFG_023).</summary>
public class MfgScrap : TenantEntity
{
    public Guid WorkOrderId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Qty { get; set; }
    public string Unit { get; set; } = "CAI";
    /// <summary>Scrap · Loss</summary>
    public string ScrapType { get; set; } = "Scrap";
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid RecordedByUserId { get; set; }
    public string? Note { get; set; }
}
