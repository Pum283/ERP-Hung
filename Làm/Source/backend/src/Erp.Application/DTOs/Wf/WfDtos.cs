namespace Erp.Application.DTOs.Wf;

public sealed record WfTaskDto(
    Guid Id,
    Guid InstanceId,
    Guid NodeId,
    string? NodeName,
    string Status,
    DateTimeOffset? DueAt,
    string SourceModule,
    string SourceDocType,
    Guid SourceDocId,
    string? DocSummary,
    Guid? AssigneeUserId = null,
    string? AssigneeName = null,
    bool ViaDelegation = false);

public sealed record WfActRequest(string Action, string? Comment);

public sealed record WfDelegationDto(
    Guid Id, Guid FromUserId, string FromUserName, Guid ToUserId, string ToUserName,
    DateOnly StartDate, DateOnly EndDate, string? ModuleCode, bool IsActive, string? Note,
    DateTimeOffset CreatedAt);

public sealed record WfDelegationUpsertRequest(
    Guid? Id, Guid ToUserId, DateOnly StartDate, DateOnly EndDate,
    string? ModuleCode, bool IsActive, string? Note);

public sealed record WfDashboardDto(
    int PendingTasks, int OverdueTasks, int CompletedToday, int RejectedToday,
    int RunningInstances, int CompletedInstances, int RejectedInstances,
    IReadOnlyList<WfModuleStatDto> ByModule,
    IReadOnlyList<WfDailyStatDto> Last7Days,
    IReadOnlyList<WfAssigneeLoadDto> TopAssignees);

public sealed record WfModuleStatDto(string ModuleCode, int Pending, int Completed, int Rejected);

public sealed record WfDailyStatDto(DateOnly Date, int Completed, int Rejected);

public sealed record WfAssigneeLoadDto(Guid UserId, string UserName, int PendingCount);
