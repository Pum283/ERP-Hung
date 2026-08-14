namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_040: Cảnh báo thất thoát
// ────────────────────────────────────────────────────────────────────────────

public record FsmSparePartLossWarningDto(
    Guid Id,
    Guid TechnicianUserId,
    string TechnicianName,
    string PartCode,
    string PartName,
    decimal IssuedQuantity,
    decimal UsedQuantity,
    decimal ReturnedQuantity,
    decimal DiscrepancyLossQty,
    string LossSeverity,
    DateTimeOffset WarningGeneratedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_043: Làm việc offline
// ────────────────────────────────────────────────────────────────────────────

public record FsmSyncOfflineDataRequest(
    Guid TechnicianUserId,
    string TechnicianName,
    string DeviceIdentifier,
    int SyncedOperationsCount,
    DateTimeOffset OfflineSessionStartedAt
);

public record FsmOfflineSyncAuditLogDto(
    Guid Id,
    Guid TechnicianUserId,
    string TechnicianName,
    string DeviceIdentifier,
    int SyncedOperationsCount,
    string SyncStatus,
    DateTimeOffset OfflineSessionStartedAt,
    DateTimeOffset SyncedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_044: Nộp quyết toán ngày
// ────────────────────────────────────────────────────────────────────────────

public record FsmSubmitDailySettlementRequest(
    Guid TechnicianUserId,
    string TechnicianName,
    decimal TotalCashCollectedVnd,
    decimal TotalOutboundExpenseVnd
);

public record FsmDailyExpenseSettlementDto(
    Guid Id,
    string SettlementVoucherNumber,
    Guid TechnicianUserId,
    string TechnicianName,
    decimal TotalCashCollectedVnd,
    decimal TotalOutboundExpenseVnd,
    decimal NetSettlementAmountVnd,
    string Status,
    DateTimeOffset SettlementDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_048: Tỷ lệ sửa lần đầu
// ────────────────────────────────────────────────────────────────────────────

public record FsmFirstTimeFixRateReportDto(
    Guid Id,
    string PeriodLabel,
    int TotalResolvedTickets,
    int FirstTimeFixCount,
    int ReopenedOrRecallCount,
    double FirstTimeFixRatePct,
    DateTimeOffset ReportGeneratedAt
);
