namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_016: Lịch SX theo xưởng/ca
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateWorkshopShiftScheduleRequest(
    string WorkshopCode,
    string ShiftCode,
    DateTimeOffset ScheduledDate,
    Guid WorkOrderId,
    string WorkOrderNumber,
    decimal TargetQuantity
);

public record MfgWorkshopShiftScheduleDto(
    Guid Id,
    string ScheduleNumber,
    string WorkshopCode,
    string ShiftCode,
    DateTimeOffset ScheduledDate,
    Guid WorkOrderId,
    string WorkOrderNumber,
    decimal TargetQuantity,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_021: Ghi nhận tiến độ công đoạn
// ────────────────────────────────────────────────────────────────────────────

public record MfgLogOperationProgressRequest(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string OperationCode,
    string OperationName,
    decimal CompletedQuantity,
    decimal DefectiveQuantity,
    string OperatorName
);

public record MfgOperationProgressTrackingDto(
    Guid Id,
    Guid WorkOrderId,
    string WorkOrderNumber,
    string OperationCode,
    string OperationName,
    decimal CompletedQuantity,
    decimal DefectiveQuantity,
    string OperatorName,
    DateTimeOffset LoggedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_026: Lệnh sản xuất lại
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateReworkWorkOrderRequest(
    Guid OriginalWorkOrderId,
    string OriginalWoNumber,
    string DefectReason,
    decimal ReworkQuantity,
    string AssignedWorkshopCode
);

public record MfgReworkWorkOrderDto(
    Guid Id,
    string ReworkWoNumber,
    Guid OriginalWorkOrderId,
    string OriginalWoNumber,
    string DefectReason,
    decimal ReworkQuantity,
    string AssignedWorkshopCode,
    string Status,
    DateTimeOffset CreatedAtDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_028: Phân bổ nhân công / chi phí chung
// ────────────────────────────────────────────────────────────────────────────

public record MfgAllocateOverheadCostRequest(
    Guid WorkOrderId,
    string WorkOrderNumber,
    decimal DirectLaborCostVnd,
    decimal MachineDepreciationCostVnd,
    decimal FactoryOverheadCostVnd,
    decimal ProducedQuantity
);

public record MfgOverheadCostAllocationDto(
    Guid Id,
    string AllocationNumber,
    Guid WorkOrderId,
    string WorkOrderNumber,
    decimal DirectLaborCostVnd,
    decimal MachineDepreciationCostVnd,
    decimal FactoryOverheadCostVnd,
    decimal TotalAllocatedCostVnd,
    decimal ProducedQuantity,
    decimal UnitCostVnd,
    DateTimeOffset AllocatedAt
);
