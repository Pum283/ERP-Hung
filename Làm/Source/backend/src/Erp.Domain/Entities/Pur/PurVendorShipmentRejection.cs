using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Từ chối lô hàng không đạt kiểm định QC (UC_PUR_036).</summary>
public class PurVendorShipmentRejection : TenantEntity
{
    public Guid PurchaseOrderId { get; set; }
    public string RejectionNumber { get; set; } = "";
    public Guid SupplierId { get; set; }
    public string RejectReason { get; set; } = "";
    public int RejectedQuantity { get; set; }
    public string QcInspectorComments { get; set; } = "";
    public string Status { get; set; } = "Quarantined"; // Quarantined | ReturnedToVendor | Scrapped
    public DateTimeOffset RejectedAt { get; set; } = DateTimeOffset.UtcNow;
}
