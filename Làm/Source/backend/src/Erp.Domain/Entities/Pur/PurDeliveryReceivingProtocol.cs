using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Biên bản giao nhận hàng hóa & Xử lý chênh lệch (UC_PUR_039 & UC_PUR_042).</summary>
public class PurDeliveryReceivingProtocol : TenantEntity
{
    public Guid GoodsReceiptNoteId { get; set; }
    public string ProtocolNumber { get; set; } = "";
    public Guid SupplierId { get; set; }
    public string DeliveryDriverName { get; set; } = "";
    public string VehiclePlateNumber { get; set; } = "";
    public int OrderedQty { get; set; }
    public int ActualReceivedQty { get; set; }
    public int DiscrepancyQty { get; set; }
    public decimal DiscrepancyAmountVnd { get; set; }
    public string DiscrepancyResolutionAction { get; set; } = "AdjustInvoiceAmount"; // AdjustInvoiceAmount | DemandSupplierReplacement | WaiveDiscrepancy
    public DateTimeOffset SignedAt { get; set; } = DateTimeOffset.UtcNow;
}
