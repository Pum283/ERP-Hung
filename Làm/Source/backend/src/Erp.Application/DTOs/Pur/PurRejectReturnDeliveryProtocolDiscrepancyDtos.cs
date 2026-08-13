namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_036: Từ chối lô hàng không đạt (QC Inspection Rejection)
// ────────────────────────────────────────────────────────────────────────────

public record PurRejectShipmentRequest(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string RejectReason,
    int RejectedQuantity,
    string QcInspectorComments
);

public record PurShipmentRejectionDto(
    Guid Id,
    Guid PurchaseOrderId,
    string RejectionNumber,
    Guid SupplierId,
    string SupplierName,
    string RejectReason,
    int RejectedQuantity,
    string QcInspectorComments,
    string Status,
    DateTimeOffset RejectedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_038: Trả hàng nhà cung cấp (RTV - Return to Vendor)
// ────────────────────────────────────────────────────────────────────────────

public record PurCreateVendorReturnRequest(
    Guid RejectionId,
    Guid SupplierId,
    decimal TotalReturnValueVnd,
    string Notes
);

public record PurVendorReturnDto(
    Guid Id,
    Guid RejectionId,
    Guid SupplierId,
    string SupplierName,
    string RtvNumber,
    decimal TotalReturnValueVnd,
    string CreditMemoStatus,
    string Notes,
    DateTimeOffset ReturnedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_039: Biên bản giao nhận & UC_PUR_042: Xử lý chênh lệch
// ────────────────────────────────────────────────────────────────────────────

public record PurCreateDeliveryProtocolRequest(
    Guid GoodsReceiptNoteId,
    Guid SupplierId,
    string DeliveryDriverName,
    string VehiclePlateNumber,
    int OrderedQty,
    int ActualReceivedQty,
    decimal UnitPriceVnd,
    string DiscrepancyResolutionAction // AdjustInvoiceAmount | DemandSupplierReplacement | WaiveDiscrepancy
);

public record PurDeliveryReceivingProtocolDto(
    Guid Id,
    Guid GoodsReceiptNoteId,
    string ProtocolNumber,
    Guid SupplierId,
    string SupplierName,
    string DeliveryDriverName,
    string VehiclePlateNumber,
    int OrderedQty,
    int ActualReceivedQty,
    int DiscrepancyQty,
    decimal DiscrepancyAmountVnd,
    string DiscrepancyResolutionAction,
    DateTimeOffset SignedAt
);
