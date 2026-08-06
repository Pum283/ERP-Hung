namespace Erp.Application.DTOs.Bi;

public sealed record BiDatasetDto(
    Guid Id, string Code, string Name, string ModuleCode, string? Description,
    string Status, DateTimeOffset? LastRefreshedAt, string? LastRefreshNote, int RowCountEstimate);
public sealed record BiDatasetUpsertRequest(
    Guid? Id, string Code, string Name, string ModuleCode, string? Description, string? Status);

public sealed record BiDatasetRefreshDto(
    Guid Id, Guid DatasetId, DateTimeOffset StartedAt, DateTimeOffset? FinishedAt,
    string Status, int RowsAffected, string? Note);
public sealed record BiRefreshRequest(string? Note);

public sealed record BiReportDto(
    Guid Id, string Code, string Name, string ModuleCode, Guid? DatasetId, string? DatasetName,
    string? Description, string? FilterSchemaJson, string Status, bool RequirePermission, int PermissionCount);
public sealed record BiReportUpsertRequest(
    Guid? Id, string Code, string Name, string ModuleCode, Guid? DatasetId,
    string? Description, string? FilterSchemaJson, string? Status, bool? RequirePermission);

public sealed record BiReportPermissionDto(
    Guid Id, Guid ReportId, string PrincipalType, string PrincipalCode, string AccessLevel);
public sealed record BiReportPermissionUpsertRequest(
    Guid? Id, Guid ReportId, string PrincipalType, string PrincipalCode, string AccessLevel);

public sealed record BiDashboardDto(
    Guid Id, string Code, string Name, string DashboardType, string? ModuleCode,
    string Status, string? Note, int SortOrder, int WidgetCount);
public sealed record BiDashboardUpsertRequest(
    Guid? Id, string Code, string Name, string DashboardType, string? ModuleCode,
    string? Status, string? Note, int? SortOrder);

public sealed record BiWidgetDto(
    Guid Id, Guid DashboardId, string Code, string Title, string WidgetType,
    string MetricKey, decimal StubValue, string? Unit, int SortOrder, string Status);
public sealed record BiWidgetUpsertRequest(
    Guid? Id, Guid DashboardId, string Code, string Title, string WidgetType,
    string MetricKey, decimal? StubValue, string? Unit, int? SortOrder, string? Status);

public sealed record BiDashboardDetailDto(BiDashboardDto Dashboard, IReadOnlyList<BiWidgetDto> Widgets);

public sealed record BiReportRunDto(
    Guid Id, Guid ReportId, string? ReportCode, string? ReportName,
    DateTimeOffset RunAt, string Status, int RowCount, string ExportFormat,
    string? ExportFileName, string? FilterJson, string? ResultPreviewJson, string? Note);
public sealed record BiReportRunRequest(string? FilterJson, string? ExportFormat);

public sealed record BiKpiTargetDto(
    Guid Id, string Code, string Name, string ModuleCode, string MetricKey, string PeriodKey,
    DateOnly PeriodFrom, DateOnly PeriodTo, decimal TargetValue, decimal ActualStubValue,
    string? Unit, string Status, string? Note,
    decimal Variance, decimal VariancePercent);
public sealed record BiKpiTargetUpsertRequest(
    Guid? Id, string Code, string Name, string ModuleCode, string MetricKey, string PeriodKey,
    DateOnly PeriodFrom, DateOnly PeriodTo, decimal TargetValue, decimal? ActualStubValue,
    string? Unit, string? Status, string? Note);

public sealed record BiAlertThresholdDto(
    Guid Id, string Code, string Name, string MetricKey, Guid? KpiTargetId, string? KpiTargetCode,
    string Operator, decimal ThresholdValue, string Severity, string Status, string? Note);
public sealed record BiAlertThresholdUpsertRequest(
    Guid? Id, string Code, string Name, string MetricKey, Guid? KpiTargetId,
    string Operator, decimal ThresholdValue, string? Severity, string? Status, string? Note);

public sealed record BiPeriodCompareRequest(
    string MetricKey, string CurrentPeriodKey, string? PriorPeriodKey, Guid? KpiTargetId);
public sealed record BiPeriodCompareDto(
    string MetricKey, string CurrentPeriodKey, decimal CurrentActual,
    string? PriorPeriodKey, decimal? PriorActual, decimal? PeriodDelta, decimal? PeriodDeltaPercent,
    decimal? TargetValue, decimal? VsTargetDelta, decimal? VsTargetPercent);

public sealed record BiTargetVsActualRowDto(
    Guid TargetId, string Code, string Name, string ModuleCode, string MetricKey, string PeriodKey,
    decimal TargetValue, decimal ActualValue, decimal Variance, decimal VariancePercent,
    string? Unit, bool Breached, string? BreachSeverity, string? BreachNote);
