namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_121: Tính hoa hồng theo kỳ
// ────────────────────────────────────────────────────────────────────────────

public record CrmCalculateCommissionRequest(
    string PeriodCode,
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate
);

public record CrmCommissionPeriodDto(
    Guid Id,
    string PeriodCode,
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalCommissionAmount,
    string Status, // Draft | Calculated | Approved | SyncedToHrmFin
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? SyncedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_122: Duyệt bảng hoa hồng
// ────────────────────────────────────────────────────────────────────────────

public record CrmApproveCommissionRequest(
    Guid PeriodId,
    Guid ApproverUserId,
    string ApprovalNotes
);

public record CrmCommissionApprovalResultDto(
    Guid PeriodId,
    string PeriodCode,
    string Status,
    Guid ApproverUserId,
    DateTimeOffset ApprovedAt,
    string ApprovalNotes
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_123: Đồng bộ hoa hồng sang HRM/FIN
// ────────────────────────────────────────────────────────────────────────────

public record CrmSyncCommissionHrmFinRequest(
    Guid PeriodId,
    bool SyncToHrmPayroll,
    bool SyncToFinAccounting
);

public record CrmCommissionSyncResultDto(
    Guid PeriodId,
    string PeriodCode,
    string Status,
    bool SyncedToHrmPayroll,
    bool SyncedToFinAccounting,
    DateTimeOffset SyncedAt,
    string Message
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_125: Bảng xếp hạng sales
// ────────────────────────────────────────────────────────────────────────────

public record CrmSalesLeaderboardEntryDto(
    Guid Id,
    Guid SalesUserId,
    string SalesUserName,
    int RankPosition,
    decimal TotalRevenue,
    int TotalNewCustomers,
    decimal TotalCommissionEarned,
    string RankingPeriod
);
