using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Xác nhận đơn hàng mua (PO) từ nhà cung cấp (UC_PUR_029).</summary>
public class PurVendorPoConfirmation : TenantEntity
{
    public Guid PurchaseOrderId { get; set; }
    public string PoNumber { get; set; } = "";
    public Guid SupplierId { get; set; }
    public string ConfirmationStatus { get; set; } = "Confirmed"; // Confirmed | ConfirmedWithChanges | Rejected
    public DateTimeOffset PromisedDeliveryDate { get; set; }
    public string VendorComments { get; set; } = "";
    public DateTimeOffset ConfirmedAt { get; set; } = DateTimeOffset.UtcNow;
}
