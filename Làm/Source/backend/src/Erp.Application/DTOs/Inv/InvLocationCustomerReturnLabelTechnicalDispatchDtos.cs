namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_013: Vị trí / kệ / bin
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateWarehouseBinLocationRequest(
    Guid WarehouseId,
    string ZoneName,
    string Aisle,
    string Rack,
    string ShelfBin
);

public record InvWarehouseBinLocationDto(
    Guid Id,
    Guid WarehouseId,
    string LocationCode,
    string ZoneName,
    string Aisle,
    string Rack,
    string ShelfBin,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_021: Nhập trả từ khách
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateCustomerReturnReceiptRequest(
    Guid CustomerId,
    Guid SalesOrderId,
    string ReturnReason,
    string InspectionCondition,
    decimal TotalRefundAmountVnd
);

public record InvCustomerReturnReceiptDto(
    Guid Id,
    string ReceiptNumber,
    Guid CustomerId,
    Guid SalesOrderId,
    string ReturnReason,
    string InspectionCondition,
    decimal TotalRefundAmountVnd,
    DateTimeOffset ReceivedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_023: In tem lô / serial
// ────────────────────────────────────────────────────────────────────────────

public record InvPrintLotSerialLabelRequest(
    Guid ProductId,
    string ProductCode,
    string LotNumber,
    string SerialNumber,
    DateTimeOffset ManufactureDate,
    DateTimeOffset ExpirationDate,
    string LabelTemplate
);

public record InvLotSerialLabelPrintDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string LotNumber,
    string SerialNumber,
    DateTimeOffset ManufactureDate,
    DateTimeOffset ExpirationDate,
    string LabelTemplate,
    DateTimeOffset PrintedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_027: Xuất cho dịch vụ kỹ thuật
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateTechnicalServiceDispatchRequest(
    Guid ServiceTicketId,
    string TechnicianName,
    Guid WarehouseId,
    decimal TotalPartsValueVnd,
    string PurposeComments
);

public record InvTechnicalServiceDispatchDto(
    Guid Id,
    string DispatchNumber,
    Guid ServiceTicketId,
    string TechnicianName,
    Guid WarehouseId,
    decimal TotalPartsValueVnd,
    string PurposeComments,
    DateTimeOffset DispatchedAt
);
