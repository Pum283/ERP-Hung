namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_035: Cách ly hàng lỗi
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateQuarantineHoldRequest(
    string LotNumber,
    string ItemCode,
    decimal QuarantinedQuantity,
    string QuarantineLocationCode,
    string DefectCategory
);

public record MfgDefectiveQuarantineHoldDto(
    Guid Id,
    string QuarantineHoldNumber,
    string LotNumber,
    string ItemCode,
    decimal QuarantinedQuantity,
    string QuarantineLocationCode,
    string DefectCategory,
    string Status,
    DateTimeOffset HoldAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_036: Báo cáo tỷ lệ đạt QC
// ────────────────────────────────────────────────────────────────────────────

public record MfgQualityYieldSummaryDto(
    int TotalInspectedLots,
    decimal TotalInspectedQty,
    decimal TotalPassedQty,
    decimal TotalRejectedQty,
    double OverallPassRatePct,
    double OverallFirstPassYieldPct
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_037: Lô/mẻ sản xuất
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateBatchLotRequest(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string ProductCode,
    decimal BatchSizePlannedQty,
    DateTimeOffset ManufacturingDate,
    DateTimeOffset ExpiryDate
);

public record MfgProductionBatchLotDto(
    Guid Id,
    string BatchNumber,
    Guid WorkOrderId,
    string WorkOrderNumber,
    string ProductCode,
    decimal BatchSizePlannedQty,
    decimal BatchSizeActualQty,
    DateTimeOffset ManufacturingDate,
    DateTimeOffset ExpiryDate,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_038: Ghi nhận thông số mẻ
// ────────────────────────────────────────────────────────────────────────────

public record MfgLogBatchParameterRequest(
    string BatchNumber,
    string ParameterName,
    decimal TargetValue,
    decimal ActualMeasuredValue,
    string UnitOfMeasure,
    bool IsWithinTolerance,
    string RecordedBy
);

public record MfgBatchProcessParameterDto(
    Guid Id,
    string BatchNumber,
    string ParameterName,
    decimal TargetValue,
    decimal ActualMeasuredValue,
    string UnitOfMeasure,
    bool IsWithinTolerance,
    string RecordedBy,
    DateTimeOffset RecordedAt
);
