namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_047: Truy vết lô xuôi/ngược
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateLotTraceRecordRequest(
    string LotNumber,
    Guid ProductId,
    string Direction,
    string OriginSupplierOrPO,
    string ProductionBatchNumber,
    string CustomerSalesOrderNumber
);

public record InvLotTraceabilityDto(
    Guid Id,
    string LotNumber,
    Guid ProductId,
    string Direction,
    string OriginSupplierOrPO,
    string ProductionBatchNumber,
    string CustomerSalesOrderNumber,
    DateTimeOffset RecordedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_051: Kiểm kê theo vị trí / nhóm
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateStocktakeLocationGroupRequest(
    Guid WarehouseId,
    string ScopeType,
    string ScopeTarget,
    int PlannedItemsCount
);

public record InvStocktakeLocationGroupDto(
    Guid Id,
    string StocktakeCode,
    Guid WarehouseId,
    string ScopeType,
    string ScopeTarget,
    int PlannedItemsCount,
    int CountedItemsCount,
    string Status,
    DateTimeOffset ScheduledDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_054: Khóa giao dịch khi đang kiểm kê
// ────────────────────────────────────────────────────────────────────────────

public record InvSetStocktakeLockRequest(
    Guid WarehouseId,
    string LockScope,
    string TargetIdentifier,
    bool IsLocked,
    string LockedBy,
    string LockReason
);

public record InvStocktakeLockDto(
    Guid Id,
    Guid WarehouseId,
    string LockScope,
    string TargetIdentifier,
    bool IsLocked,
    string LockedBy,
    string LockReason,
    DateTimeOffset LockedAt,
    DateTimeOffset? UnlockedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_056: Đề nghị xuất nội bộ
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateInternalIssueRequest(
    string RequestingDepartment,
    string Purpose,
    Guid WarehouseId,
    decimal EstimatedTotalCostVnd
);

public record InvInternalIssueRequestDto(
    Guid Id,
    string RequestNumber,
    string RequestingDepartment,
    string Purpose,
    Guid WarehouseId,
    decimal EstimatedTotalCostVnd,
    string Status,
    DateTimeOffset RequestedAt
);
