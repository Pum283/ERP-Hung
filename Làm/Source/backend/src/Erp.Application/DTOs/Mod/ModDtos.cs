namespace Erp.Application.DTOs.Mod;

public sealed record ModMasterDto(Guid Id, string ModuleCode, string RecordType, string Code, string Name, string Status, string? PayloadJson);

public sealed record ModMasterUpsertRequest(Guid? Id, string RecordType, string Code, string Name, string Status, string? PayloadJson);

public sealed record ModDocumentDto(Guid Id, string ModuleCode, string DocType, string DocNo, string Title, string Status, Guid? OwnerUserId, Guid? RefMasterId, string? PayloadJson, DateTimeOffset CreatedAt);

public sealed record ModDocumentUpsertRequest(Guid? Id, string DocType, string? DocNo, string Title, string Status, Guid? OwnerUserId, Guid? RefMasterId, string? PayloadJson);

public sealed record WorkTypeDto(Guid Id, string Code, string Name, bool IsActive);
public sealed record WorkProjectDto(Guid Id, string Code, string Name, bool IsActive);
public sealed record WorkItemDto(Guid Id, string Kind, string Title, string? Description, Guid? ProjectId, Guid? AssigneeUserId, Guid? ReporterUserId, DateTimeOffset? DueAt, string Status, string Priority);

public sealed record WorkItemUpsertRequest(Guid? Id, string Kind, string Title, string? Description, Guid? ProjectId, Guid? AssigneeUserId, DateTimeOffset? DueAt, string Status, string Priority);

public sealed record EmploymentStatusChangeDto(Guid Id, Guid EmployeeId, string FromStatus, string ToStatus, DateOnly EffectiveDate, string? Reason, Guid? OrgUnitId, Guid? DepartmentId, Guid? JobTitleId);

public sealed record ChangeEmploymentStatusRequest(string ToStatus, DateOnly EffectiveDate, string? Reason, Guid? OrgUnitId, Guid? DepartmentId, Guid? JobTitleId);
