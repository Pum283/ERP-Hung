using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Cấu hình thiết bị quét mã vạch / QR Code POS (UC_POS_006).</summary>
public class PosBarcodeScannerConfig : TenantEntity
{
    public string ScannerName { get; set; } = "";
    /// <summary>USB_HID | USB_COM | Bluetooth | SerialRS232</summary>
    public string ConnectionType { get; set; } = "USB_HID";
    public string PrefixKey { get; set; } = "";
    public string SuffixKey { get; set; } = "ENTER";
    public int ScanTimeoutMs { get; set; } = 300;
    public bool IsActive { get; set; } = true;
}
