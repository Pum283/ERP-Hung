namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_039: Đóng gói & gắn tem
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreatePackagingLabelRequest(
    string ProductCode,
    string PackagingType,
    decimal UnitsPerPackage,
    string BarcodeLabelFormat,
    string LabelTemplatePath
);

public record MfgPackagingLabelTagDto(
    Guid Id,
    string ProductCode,
    string PackagingType,
    decimal UnitsPerPackage,
    string BarcodeLabelFormat,
    string LabelTemplatePath,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_040: Định mức phối trộn
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateBlendingRecipeRequest(
    string RecipeCode,
    string RecipeName,
    string IngredientProductCode,
    string IngredientProductName,
    decimal MixingRatioPercentage,
    decimal TolerancePercentage,
    string MixingOrderStep
);

public record MfgBlendingRecipeRatioDto(
    Guid Id,
    string RecipeCode,
    string RecipeName,
    string IngredientProductCode,
    string IngredientProductName,
    decimal MixingRatioPercentage,
    decimal TolerancePercentage,
    string MixingOrderStep
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_044: Hiệu suất / OEE
// ────────────────────────────────────────────────────────────────────────────

public record MfgCalculateOeeRequest(
    string WorkCenterCode,
    string WorkCenterName,
    double AvailabilityRatePct,
    double PerformanceRatePct,
    double QualityRatePct
);

public record MfgOverallEquipmentEffectivenessDto(
    Guid Id,
    string WorkCenterCode,
    string WorkCenterName,
    double AvailabilityRatePct,
    double PerformanceRatePct,
    double QualityRatePct,
    double OverallOeePct,
    DateTimeOffset CalculationPeriod
);
