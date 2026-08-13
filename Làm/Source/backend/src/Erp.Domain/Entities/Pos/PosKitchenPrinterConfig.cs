using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Cấu hình máy in bếp/khu vực POS (UC_POS_004).</summary>
public class PosKitchenPrinterConfig : TenantEntity
{
    public string PrinterName { get; set; } = "";
    /// <summary>Kitchen | Bar | Bakery | Cashier</summary>
    public string Area { get; set; } = "Kitchen";
    /// <summary>LAN_IP | USB | Serial | Bluetooth</summary>
    public string ConnectionType { get; set; } = "LAN_IP";
    public string IpAddressOrPort { get; set; } = "192.168.1.200";
    public int PaperWidthMm { get; set; } = 80;
    public bool AutoCutPaper { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
