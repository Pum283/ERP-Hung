using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Barcode / QR Code sản phẩm (UC_INV_009).</summary>
public class InvProductBarcodeQr : TenantEntity
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string BarcodeEan13 { get; set; } = "";
    public string QrCodePayload { get; set; } = "";
    public string PrintableLabelTemplate { get; set; } = "Standard-50x30mm";
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
