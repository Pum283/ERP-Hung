namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_060: Đồng bộ đơn POS sang CRM
// ────────────────────────────────────────────────────────────────────────────

public record PosSyncOrderToCrmRequest(
    Guid PosOrderId,
    Guid CustomerId
);

public record PosSyncOrderToCrmResultDto(
    Guid PosOrderId,
    Guid CustomerId,
    bool IsSynced,
    string CrmActivityRecordCode,
    DateTimeOffset SyncedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_002: Phân loại nhóm nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurSaveSupplierCategoryRequest(
    string CategoryCode,
    string CategoryName,
    string Description
);

public record PurSupplierCategoryDto(
    Guid Id,
    string CategoryCode,
    string CategoryName,
    string Description,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_004: Lead time & MOQ
// ────────────────────────────────────────────────────────────────────────────

public record PurSupplierLeadTimeMoqDto(
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    int DeliveryLeadTimeDays,
    int MinimumOrderQuantity,
    decimal MinimumOrderValueVnd
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_005: Đánh giá chất lượng nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurSaveSupplierQualityEvaluationRequest(
    Guid SupplierId,
    string Period,
    double OnTimeDeliveryScore,
    double QualityComplianceScore,
    double PriceCompetitivenessScore,
    string Comments
);

public record PurSupplierQualityEvaluationDto(
    Guid Id,
    Guid SupplierId,
    string Period,
    double OnTimeDeliveryScore,
    double QualityComplianceScore,
    double PriceCompetitivenessScore,
    double OverallRatingScore,
    string RatingGrade, // A | B | C | D
    string Comments,
    DateTimeOffset EvaluatedAt
);
