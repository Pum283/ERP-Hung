namespace Erp.Application.DTOs.Pjm;

public sealed record PjmProjectTypeDto(Guid Id, string Code, string Name, string Status, string? Note);
public sealed record PjmProjectTypeUpsertRequest(Guid? Id, string Code, string Name, string? Status, string? Note);

public sealed record PjmProjectStatusDto(Guid Id, string Code, string Name, int SortOrder, bool IsTerminal, bool IsActive);
public sealed record PjmProjectStatusUpsertRequest(
    Guid? Id, string Code, string Name, int? SortOrder, bool? IsTerminal, bool? IsActive);

public sealed record PjmWbsTemplateDto(Guid Id, string Code, string Name, string Status, string? Note, int ItemCount);
public sealed record PjmWbsTemplateUpsertRequest(Guid? Id, string Code, string Name, string? Status, string? Note);
public sealed record PjmWbsTemplateItemDto(
    Guid Id, Guid TemplateId, string Code, string Name, Guid? ParentItemId, int SortOrder);
public sealed record PjmWbsTemplateItemUpsertRequest(
    Guid? Id, string Code, string Name, Guid? ParentItemId, int? SortOrder);
public sealed record PjmWbsTemplateDetailDto(PjmWbsTemplateDto Template, IReadOnlyList<PjmWbsTemplateItemDto> Items);

public sealed record PjmProjectDto(
    Guid Id, string Code, string Name, Guid? ProjectTypeId, string? ProjectTypeName,
    string StatusCode, string? StatusName, string? CustomerName, string? ContractCode,
    string? SourceOpportunityCode, Guid? PmUserId, string? PmName, decimal Budget,
    DateTimeOffset? StartDate, DateTimeOffset? EndDate, string? Note,
    int MemberCount, int WbsCount,
    decimal ActualCost, decimal RecognizedRevenue, decimal Margin,
    DateTimeOffset? ClosedAt);

public sealed record PjmProjectUpsertRequest(
    Guid? Id, string? Code, string Name, Guid? ProjectTypeId, string? StatusCode,
    string? CustomerName, string? ContractCode, string? SourceOpportunityCode,
    Guid? PmUserId, string? PmName, decimal? Budget,
    DateTimeOffset? StartDate, DateTimeOffset? EndDate, string? Note,
    Guid? ApplyTemplateId);

public sealed record PjmProjectMemberDto(
    Guid Id, Guid ProjectId, Guid UserId, string? UserName, string Role, bool IsActive,
    decimal AllocationPct, DateTimeOffset? FromDate, DateTimeOffset? ToDate);
public sealed record PjmProjectMemberUpsertRequest(
    Guid? Id, Guid UserId, string Role, bool? IsActive,
    decimal? AllocationPct, DateTimeOffset? FromDate, DateTimeOffset? ToDate);

public sealed record PjmWbsItemDto(
    Guid Id, Guid ProjectId, string Code, string Name, Guid? ParentItemId,
    Guid? AssigneeUserId, string? AssigneeName, string Status, int SortOrder, string? Note,
    decimal PercentComplete, bool IsMilestone, DateTimeOffset? DueDate, bool IsOverdue);
public sealed record PjmWbsItemUpsertRequest(
    Guid? Id, string Code, string Name, Guid? ParentItemId,
    Guid? AssigneeUserId, string? AssigneeName, string? Status, int? SortOrder, string? Note,
    decimal? PercentComplete, bool? IsMilestone, DateTimeOffset? DueDate);

public sealed record PjmProjectDetailDto(
    PjmProjectDto Project,
    IReadOnlyList<PjmProjectMemberDto> Members,
    IReadOnlyList<PjmWbsItemDto> WbsItems,
    IReadOnlyList<PjmExpenseDto> Expenses,
    IReadOnlyList<PjmMaterialIssueDto> MaterialIssues,
    IReadOnlyList<PjmAcceptanceDto> Acceptances,
    PjmCostSummaryDto CostSummary);

public sealed record PjmExpenseDto(
    Guid Id, Guid ProjectId, string Code, string Category, string Description,
    decimal Amount, DateTimeOffset ExpenseDate, Guid? WbsItemId, string Status,
    DateTimeOffset? PostedAt, string? Note);
public sealed record PjmExpenseUpsertRequest(
    Guid? Id, string Category, string Description, decimal Amount,
    DateTimeOffset? ExpenseDate, Guid? WbsItemId, string? Note, bool Post);

public sealed record PjmMaterialIssueLineDto(
    Guid Id, string ProductCode, string ProductName, string Unit, decimal Qty, decimal UnitCost, decimal Amount);
public sealed record PjmMaterialIssueDto(
    Guid Id, Guid ProjectId, string Code, string Status, string? Note, DateTimeOffset? PostedAt,
    decimal TotalAmount, IReadOnlyList<PjmMaterialIssueLineDto> Lines);
public sealed record PjmMaterialIssueLineRequest(
    string ProductCode, string ProductName, string? Unit, decimal Qty, decimal UnitCost);
public sealed record PjmMaterialIssueCreateRequest(
    string? Note, bool Post, IReadOnlyList<PjmMaterialIssueLineRequest> Lines);

public sealed record PjmAcceptanceDto(
    Guid Id, Guid ProjectId, string Code, string Kind, string Title, string Status,
    string? SignerName, DateTimeOffset? SignedAt, string? Note);
public sealed record PjmAcceptanceCreateRequest(string Kind, string Title, string? Note);
public sealed record PjmAcceptanceSignRequest(string SignerName, string? Note);

public sealed record PjmRecognizeRevenueRequest(decimal Amount, string? Note);
public sealed record PjmCloseProjectRequest(string? Note);

public sealed record PjmCostSummaryDto(
    decimal Budget, decimal ExpenseCost, decimal MaterialCost, decimal ActualCost,
    decimal RecognizedRevenue, decimal Margin, decimal BudgetVariance, bool HasFinalAcceptance);
