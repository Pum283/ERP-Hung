using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Cấu hình ngăn kéo tiền mặt POS (UC_POS_005).</summary>
public class PosCashDrawerConfig : TenantEntity
{
    public string DrawerName { get; set; } = "";
    /// <summary>PrinterKickout | DirectUSB | SerialRelay</summary>
    public string TriggerMode { get; set; } = "PrinterKickout";
    public string OpenPulseCommandHex { get; set; } = "1B700019FA";
    public bool AutoOpenOnCashPayment { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
