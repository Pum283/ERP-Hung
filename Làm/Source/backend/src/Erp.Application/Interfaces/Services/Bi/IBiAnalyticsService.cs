using Erp.Application.DTOs.Bi;

namespace Erp.Application.Interfaces.Services.Bi;

public interface IBiAnalyticsService
{
    Task<IReadOnlyList<BiDatasetDto>> ListDatasetsAsync(Guid tenantId, string? moduleCode, CancellationToken ct = default);
    Task<BiDatasetDto> UpsertDatasetAsync(Guid tenantId, Guid userId, BiDatasetUpsertRequest req, CancellationToken ct = default);
    Task<BiDatasetDto> RefreshDatasetAsync(Guid tenantId, Guid userId, Guid datasetId, BiRefreshRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<BiDatasetRefreshDto>> ListRefreshesAsync(Guid tenantId, Guid datasetId, CancellationToken ct = default);

    Task<IReadOnlyList<BiReportDto>> ListReportsAsync(Guid tenantId, string? moduleCode, CancellationToken ct = default);
    Task<BiReportDto> UpsertReportAsync(Guid tenantId, Guid userId, BiReportUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<BiReportPermissionDto>> ListPermissionsAsync(Guid tenantId, Guid reportId, CancellationToken ct = default);
    Task<BiReportPermissionDto> UpsertPermissionAsync(Guid tenantId, Guid userId, BiReportPermissionUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<BiDashboardDto>> ListDashboardsAsync(Guid tenantId, CancellationToken ct = default);
    Task<BiDashboardDetailDto> GetDashboardDetailAsync(Guid tenantId, Guid dashboardId, CancellationToken ct = default);
    Task<BiDashboardDto> UpsertDashboardAsync(Guid tenantId, Guid userId, BiDashboardUpsertRequest req, CancellationToken ct = default);
    Task<BiWidgetDto> UpsertWidgetAsync(Guid tenantId, Guid userId, BiWidgetUpsertRequest req, CancellationToken ct = default);

    Task<BiReportRunDto> RunReportAsync(Guid tenantId, Guid userId, Guid reportId, BiReportRunRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<BiReportRunDto>> ListRunsAsync(Guid tenantId, Guid? reportId, CancellationToken ct = default);

    /// <summary>UC_BI_016 — tải nội dung xuất thật (CSV cho Excel, text cho Pdf) từ lần chạy.</summary>
    Task<(string FileName, string ContentType, string Content)> DownloadRunExportAsync(Guid tenantId, Guid runId, CancellationToken ct = default);

    Task<IReadOnlyList<BiKpiTargetDto>> ListKpiTargetsAsync(
        Guid tenantId, string? periodKey = null, string? moduleCode = null, CancellationToken ct = default);
    Task<BiKpiTargetDto> UpsertKpiTargetAsync(
        Guid tenantId, Guid userId, BiKpiTargetUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<BiAlertThresholdDto>> ListAlertThresholdsAsync(Guid tenantId, CancellationToken ct = default);
    Task<BiAlertThresholdDto> UpsertAlertThresholdAsync(
        Guid tenantId, Guid userId, BiAlertThresholdUpsertRequest req, CancellationToken ct = default);

    Task<BiPeriodCompareDto> ComparePeriodsAsync(
        Guid tenantId, BiPeriodCompareRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<BiTargetVsActualRowDto>> ListTargetVsActualAsync(
        Guid tenantId, string periodKey, string? moduleCode = null, CancellationToken ct = default);
}
