namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_111: Chặn bán khi vượt công nợ
// ────────────────────────────────────────────────────────────────────────────

public record CrmCheckCreditLimitRequest(
    Guid CustomerId,
    decimal NewOrderValue
);

public record CrmCreditCheckResultDto(
    Guid CustomerId,
    string CustomerName,
    decimal CurrentDebtBalance,
    decimal CreditLimit,
    decimal NewOrderValue,
    decimal ProjectedDebtBalance,
    bool IsCreditLimitExceeded,
    string DecisionMessage // Approved | Blocked
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_114: Chuyển ticket sang FSM
// ────────────────────────────────────────────────────────────────────────────

public record CrmTransferTicketToFsmRequest(
    Guid TicketId,
    Guid FsmTechnicianId,
    string Priority, // Low | Normal | High | Urgent
    string MaintenanceNotes
);

public record CrmFsmTicketHandoffDto(
    Guid TicketId,
    string TicketCode,
    Guid FsmTechnicianId,
    string FsmTechnicianName,
    string Priority,
    string Status, // TransferredToFsm | InProgress | Completed
    string MaintenanceNotes,
    DateTimeOffset TransferredAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_115: Lịch chăm sóc / nhắc tái mua
// ────────────────────────────────────────────────────────────────────────────

public record CrmScheduleCustomerCareRequest(
    Guid CustomerId,
    string CareType, // RoutineCheck | RepurchaseReminder | PostServiceFollowUp
    DateTime ScheduledDate,
    string Notes,
    Guid? AssignedUserId
);

public record CrmCustomerCareScheduleDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CareType,
    DateTime ScheduledDate,
    string Status, // Pending | Completed | Cancelled
    string Notes,
    Guid? AssignedUserId
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_116: Chương trình loyalty
// ────────────────────────────────────────────────────────────────────────────

public record CrmCreateLoyaltyProgramRequest(
    string ProgramCode,
    string ProgramName,
    decimal PointsPerVnd,
    int MinPointsToRedeem,
    string Description
);

public record CrmLoyaltyProgramDto(
    Guid Id,
    string ProgramCode,
    string ProgramName,
    decimal PointsPerVnd,
    int MinPointsToRedeem,
    bool IsActive,
    string Description,
    int TotalEnrolledCustomers
);
