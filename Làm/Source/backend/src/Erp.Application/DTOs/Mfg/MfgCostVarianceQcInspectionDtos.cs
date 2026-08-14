namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_030: Đối chiếu lý thuyết vs thực tế
// ────────────────────────────────────────────────────────────────────────────

public record MfgAnalyzeCostVarianceRequest(
    Guid WorkOrderId,
    string WorkOrderNumber,
    decimal StandardTheoreticalCostVnd,
    decimal ActualIncurredCostVnd,
    string VarianceRootCause
);

public record MfgCostVarianceAnalysisDto(
    Guid Id,
    string AnalysisNumber,
    Guid WorkOrderId,
    string WorkOrderNumber,
    decimal StandardTheoreticalCostVnd,
    decimal ActualIncurredCostVnd,
    decimal CostVarianceVnd,
    double VariancePercentage,
    string VarianceRootCause,
    DateTimeOffset AnalyzedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_032: Tiêu chí QC đầu vào (IQC)
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateIncomingQcCriterionRequest(
    string CriterionCode,
    string CriterionName,
    string MaterialGroup,
    string StandardSpecification,
    string InspectionMethod,
    decimal MinAcceptableValue,
    decimal MaxAcceptableValue,
    bool IsMandatory
);

public record MfgIncomingQcCriterionDto(
    Guid Id,
    string CriterionCode,
    string CriterionName,
    string MaterialGroup,
    string StandardSpecification,
    string InspectionMethod,
    decimal MinAcceptableValue,
    decimal MaxAcceptableValue,
    bool IsMandatory
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_033: QC thành phẩm (FQC)
// ────────────────────────────────────────────────────────────────────────────

public record MfgPerformFinishedGoodsQcRequest(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string FinishedProductCode,
    decimal SampleSizeQty,
    decimal DefectFoundQty,
    string InspectionResult,
    string InspectorName
);

public record MfgFinishedGoodsQcCheckDto(
    Guid Id,
    string InspectionNumber,
    Guid WorkOrderId,
    string WorkOrderNumber,
    string FinishedProductCode,
    decimal SampleSizeQty,
    decimal DefectFoundQty,
    string InspectionResult,
    string InspectorName,
    DateTimeOffset InspectedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_034: Ghi nhận lô đạt / không đạt
// ────────────────────────────────────────────────────────────────────────────

public record MfgDecideLotDispositionRequest(
    string LotNumber,
    string ItemCode,
    decimal TotalLotQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    string DispositionDecision,
    string QualityManagerNote
);

public record MfgInspectionLotDispositionDto(
    Guid Id,
    string LotNumber,
    string ItemCode,
    decimal TotalLotQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    string DispositionDecision,
    string QualityManagerNote,
    DateTimeOffset DecidedAt
);
