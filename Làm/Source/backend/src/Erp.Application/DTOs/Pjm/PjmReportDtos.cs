namespace Erp.Application.DTOs.Pjm;

public sealed record PjmPortfolioRowDto(
    Guid ProjectId, string Code, string Name, string StatusCode, string? PmName,
    decimal Budget, DateTimeOffset? StartDate, DateTimeOffset? EndDate,
    decimal ProgressPercent, string Health, int WbsCount, int OverdueCount, int MilestoneCount);

public sealed record PjmProgressHealthRowDto(
    Guid ProjectId, string Code, string Name, string StatusCode,
    decimal ProgressPercent, string Health, int OpenWbs, int DoneWbs,
    int OverdueWbs, int OverdueMilestones, DateTimeOffset? EndDate, bool ProjectEndOverdue);

public sealed record PjmOverdueRowDto(
    Guid ProjectId, string ProjectCode, string ProjectName,
    Guid WbsItemId, string WbsCode, string WbsName,
    bool IsMilestone, DateTimeOffset DueDate, decimal PercentComplete, string? AssigneeName);

public sealed record PjmDashboardDto(
    int ActiveCount, int DraftCount, int ClosedCount,
    int OverdueProjectCount, int OverdueWbsCount, int OverdueMilestoneCount,
    decimal AvgActiveProgressPercent);

public sealed record PjmProfitRowDto(
    Guid ProjectId, string Code, string Name, string StatusCode,
    decimal Budget, decimal ActualCost, decimal RecognizedRevenue, decimal Margin, decimal MarginPct,
    decimal BudgetVariance, bool OverBudget);
