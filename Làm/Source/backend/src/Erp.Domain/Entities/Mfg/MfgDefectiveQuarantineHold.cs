using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Lệnh cách ly & niêm phong hàng lỗi (UC_MFG_035).</summary>
public class MfgDefectiveQuarantineHold : TenantEntity
{
    public string QuarantineHoldNumber { get; set; } = "";
    public string LotNumber { get; set; } = "";
    public string ItemCode { get; set; } = "";
    public decimal QuarantinedQuantity { get; set; }
    public string QuarantineLocationCode { get; set; } = "KHO-CACH-LY-01";
    public string DefectCategory { get; set; } = "Nứt vỡ cấu trúc cơ khí";
    public string Status { get; set; } = "UnderQuarantine"; // UnderQuarantine | Released | Destroyed
    public DateTimeOffset HoldAt { get; set; } = DateTimeOffset.UtcNow;
}
