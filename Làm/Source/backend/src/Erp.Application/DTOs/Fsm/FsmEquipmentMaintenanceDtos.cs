namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_033: Lịch bảo trì theo thiết bị
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateMaintenanceScheduleRequest(
    Guid AssetId,
    string SerialNumber,
    string ModelName,
    string CustomerName,
    string MaintenanceFrequency,
    DateTimeOffset NextDueDate,
    bool AutoGenerateTicket
);

public record FsmEquipmentMaintenanceScheduleDto(
    Guid Id,
    Guid AssetId,
    string SerialNumber,
    string ModelName,
    string CustomerName,
    string MaintenanceFrequency,
    DateTimeOffset NextDueDate,
    bool AutoGenerateTicket,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_034: Tự tạo ticket bảo trì đến hạn
// ────────────────────────────────────────────────────────────────────────────

public record FsmAutoDueMaintenanceTicketDto(
    Guid Id,
    string GeneratedTicketNumber,
    Guid AssetId,
    string SerialNumber,
    string CustomerName,
    string MaintenanceType,
    DateTimeOffset ScheduledServiceDate,
    string Status,
    DateTimeOffset GeneratedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_035: Checklist bảo trì chuẩn
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateStandardChecklistItemRequest(
    string EquipmentCategory,
    string ChecklistItemName,
    string StandardOperatingProcedure,
    int SequenceOrder,
    bool IsMandatory
);

public record FsmStandardMaintenanceChecklistDto(
    Guid Id,
    string EquipmentCategory,
    string ChecklistItemName,
    string StandardOperatingProcedure,
    int SequenceOrder,
    bool IsMandatory
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_036: Báo cáo thực hiện bảo trì
// ────────────────────────────────────────────────────────────────────────────

public record FsmMaintenanceExecutionReportDto(
    Guid Id,
    string PeriodLabel,
    int TotalScheduledVisits,
    int CompletedVisitsCount,
    int DelayedVisitsCount,
    double OnTimeCompletionRatePct,
    decimal TotalMaintenanceRevenueVnd,
    DateTimeOffset ReportGeneratedAt
);
