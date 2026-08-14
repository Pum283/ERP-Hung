using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Ghi nhận quyết định xử lý lô đạt / không đạt (UC_MFG_034).</summary>
public class MfgInspectionLotDisposition : TenantEntity
{
    public string LotNumber { get; set; } = "";
    public string ItemCode { get; set; } = "";
    public decimal TotalLotQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string DispositionDecision { get; set; } = "ReleaseToStock"; // ReleaseToStock | Quarantine | Scrapped | Rework
    public string QualityManagerNote { get; set; } = "Lô đạt tiêu chuẩn ISO, cho phép nhập kho";
    public DateTimeOffset DecidedAt { get; set; } = DateTimeOffset.UtcNow;
}
