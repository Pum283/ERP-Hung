namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_117: Tích điểm / đổi quà
// ────────────────────────────────────────────────────────────────────────────

public record CrmRedeemRewardRequest(
    Guid CustomerId,
    string RewardItemName,
    int PointsRedeemed
);

public record CrmRewardRedemptionDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string RewardItemName,
    int PointsRedeemed,
    string Status, // Pending | Fulfilled | Cancelled
    DateTimeOffset RedeemedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_118: Khảo sát hài lòng
// ────────────────────────────────────────────────────────────────────────────

public record CrmSubmitSurveyResponseRequest(
    Guid CustomerId,
    int RatingScore, // 1 to 5
    string FeedbackComments,
    string ServiceChannel // StoreVisit | OnlineOrder | TechSupport
);

public record CrmCustomerSurveyResponseDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    int RatingScore,
    string FeedbackComments,
    string ServiceChannel,
    DateTimeOffset SubmittedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_119: Báo cáo retention / tái mua
// ────────────────────────────────────────────────────────────────────────────

public record CrmCustomerRetentionReportDto(
    int TotalActiveCustomers,
    int RepeatPurchasingCustomers,
    double RepeatPurchaseRatePercent,
    double CustomerChurnRatePercent,
    decimal AverageLifetimeValueVnd
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_120: Cấu hình rule hoa hồng
// ────────────────────────────────────────────────────────────────────────────

public record CrmConfigureCommissionRuleRequest(
    string RuleCode,
    string RuleName,
    string SalesRole, // FieldSales | InsideSales | AccountManager
    decimal MinRevenueThreshold,
    decimal CommissionRatePercent
);

public record CrmCommissionRuleDto(
    Guid Id,
    string RuleCode,
    string RuleName,
    string SalesRole,
    decimal MinRevenueThreshold,
    decimal CommissionRatePercent,
    bool IsActive
);
