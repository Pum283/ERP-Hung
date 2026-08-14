namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_009: Định mức hao hụt
// ────────────────────────────────────────────────────────────────────────────

public record MfgSetBomScrapAllowanceRequest(
    Guid BomId,
    string BomCode,
    Guid MaterialProductId,
    string MaterialProductCode,
    string MaterialProductName,
    decimal BaseNetQuantity,
    decimal ScrapAllowancePct,
    string Reason
);

public record MfgBomScrapAllowanceDto(
    Guid Id,
    Guid BomId,
    string BomCode,
    Guid MaterialProductId,
    string MaterialProductCode,
    string MaterialProductName,
    decimal BaseNetQuantity,
    decimal ScrapAllowancePct,
    decimal GrossPlannedQuantity,
    string Reason
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_011: Sao chép BOM
// ────────────────────────────────────────────────────────────────────────────

public record MfgCopyBomRequest(
    Guid SourceBomId,
    string SourceBomCode,
    string SourceVersion,
    string NewVersion,
    string CopiedBy
);

public record MfgBomCopyLogDto(
    Guid Id,
    Guid SourceBomId,
    string SourceBomCode,
    string SourceVersion,
    Guid NewBomId,
    string NewBomCode,
    string NewVersion,
    int CopiedLinesCount,
    string CopiedBy,
    DateTimeOffset CopiedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_012: Kế hoạch SX theo nhu cầu (MPS)
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateDemandProductionPlanRequest(
    string PlanName,
    Guid FinishedProductId,
    string ProductCode,
    string ProductName,
    decimal SalesForecastDemandQty,
    decimal BacklogOrdersDemandQty,
    string PlanningHorizon
);

public record MfgDemandProductionPlanDto(
    Guid Id,
    string PlanNumber,
    string PlanName,
    Guid FinishedProductId,
    string ProductCode,
    string ProductName,
    decimal SalesForecastDemandQty,
    decimal BacklogOrdersDemandQty,
    decimal PlannedProductionQty,
    string PlanningHorizon,
    string Status,
    DateTimeOffset CreatedAtDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_014: Tính nhu cầu nguyên vật liệu (MRP)
// ────────────────────────────────────────────────────────────────────────────

public record MfgRunMrpCalculationRequest(
    Guid MaterialProductId,
    string MaterialProductCode,
    string MaterialProductName,
    decimal GrossRequirementQty,
    decimal CurrentStockOnHandQty,
    decimal ScheduledReceiptsPoQty,
    DateTimeOffset RequiredDate
);

public record MfgMaterialRequirementPlanningDto(
    Guid Id,
    string MrpRunNumber,
    Guid MaterialProductId,
    string MaterialProductCode,
    string MaterialProductName,
    decimal GrossRequirementQty,
    decimal CurrentStockOnHandQty,
    decimal ScheduledReceiptsPoQty,
    decimal NetRequirementQty,
    decimal SuggestedPurchaseOrderQty,
    DateTimeOffset RequiredDate,
    DateTimeOffset CalculatedAt
);
