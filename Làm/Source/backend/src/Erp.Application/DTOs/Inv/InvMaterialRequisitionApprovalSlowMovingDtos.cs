namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_057, UC_INV_058, UC_INV_059: Đề nghị cấp hàng, duyệt và chuyển thành phiếu xuất
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateMaterialRequisitionRequest(
    string RequesterName,
    string DepartmentName,
    Guid WarehouseId,
    Guid ProductId,
    decimal RequestedQuantity
);

public record InvDecideMaterialRequisitionRequest(
    Guid RequisitionId,
    bool IsApproved,
    string ApproverName
);

public record InvConvertRequisitionToIssueRequest(
    Guid RequisitionId
);

public record InvMaterialRequisitionDto(
    Guid Id,
    string RequisitionNumber,
    string RequesterName,
    string DepartmentName,
    Guid WarehouseId,
    Guid ProductId,
    decimal RequestedQuantity,
    string Status,
    string ApproverName,
    string ConvertedIssueNumber,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ApprovedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_066: Hàng chậm luân chuyển
// ────────────────────────────────────────────────────────────────────────────

public record InvSlowMovingItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal CurrentStockQuantity,
    int DaysWithoutIssueMovement,
    decimal TiedUpCapitalVnd,
    string RiskLevel
);

public record InvSlowMovingSummaryDto(
    int TotalSlowMovingSkus,
    decimal TotalTiedUpCapitalVnd,
    IReadOnlyList<InvSlowMovingItemDto> Items
);
