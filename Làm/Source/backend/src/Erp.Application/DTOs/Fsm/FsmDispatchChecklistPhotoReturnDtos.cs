namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_016: Phân công theo rule
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateAutoDispatchRuleRequest(
    string RuleName,
    string TerritoryCode,
    string RequiredSkillCode,
    int MaxActiveTicketsPerTech,
    bool AutoAssignOnTicketCreation
);

public record FsmAutoDispatchRuleDto(
    Guid Id,
    string RuleName,
    string TerritoryCode,
    string RequiredSkillCode,
    int MaxActiveTicketsPerTech,
    bool AutoAssignOnTicketCreation,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_021: Checklist công việc
// ────────────────────────────────────────────────────────────────────────────

public record FsmAddChecklistStepRequest(
    Guid TicketId,
    string TicketNumber,
    string StepDescription,
    bool IsMandatory
);

public record FsmJobExecutionChecklistDto(
    Guid Id,
    Guid TicketId,
    string TicketNumber,
    string StepDescription,
    bool IsMandatory,
    bool IsCompleted,
    string CompletedByTechnicianName,
    DateTimeOffset? CompletedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_023: Chụp ảnh trước/sau
// ────────────────────────────────────────────────────────────────────────────

public record FsmUploadJobPhotoRequest(
    Guid TicketId,
    string TicketNumber,
    string PhotoType,
    string PhotoUrl,
    string Caption
);

public record FsmJobPhotoAttachmentDto(
    Guid Id,
    Guid TicketId,
    string TicketNumber,
    string PhotoType,
    string PhotoUrl,
    string Caption,
    DateTimeOffset UploadedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_025: Hoàn linh kiện thừa
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateSparePartReturnRequest(
    Guid TicketId,
    string TicketNumber,
    string PartCode,
    string PartName,
    decimal ReturnedQuantity,
    string Reason,
    string DestinationWarehouseCode
);

public record FsmSparePartReturnDto(
    Guid Id,
    string ReturnSlipNumber,
    Guid TicketId,
    string TicketNumber,
    string PartCode,
    string PartName,
    decimal ReturnedQuantity,
    string Reason,
    string DestinationWarehouseCode,
    string Status,
    DateTimeOffset ReturnedAt
);
