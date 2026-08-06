using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Phiếu nhận hàng GRN (UC_PUR_034–037).</summary>
public class PurGoodsReceipt : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid PoId { get; set; }
    public Guid VendorId { get; set; }
    /// <summary>Draft · Posted · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? QualityNote { get; set; }
    /// <summary>None · Pushed · Failed</summary>
    public string InventoryPushStatus { get; set; } = "None";
    public string? Note { get; set; }
}
