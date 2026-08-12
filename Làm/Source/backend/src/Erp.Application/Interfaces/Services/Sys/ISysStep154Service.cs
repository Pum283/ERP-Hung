using Erp.Application.DTOs.Sys;

namespace Erp.Application.Interfaces.Services.Sys;

public interface ISysStep154Service
{
    // 064 prefs
    Task<SysNotificationPreferenceDto> GetMyNotificationPreferencesAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<SysNotificationPreferenceDto> UpsertMyNotificationPreferencesAsync(Guid tenantId, Guid userId, SysNotificationPreferenceUpsertRequest req, CancellationToken ct = default);
    bool ShouldDeliverInApp(SysNotificationPreferenceDto prefs, string eventType, DateTimeOffset utcNow);

    // 071 scan
    Task<SysFileScanStatusDto> ScanFileAsync(Guid tenantId, Guid userId, Guid fileId, string? contentHint, CancellationToken ct = default);
    Task<SysFileScanStatusDto> GetFileScanStatusAsync(Guid tenantId, Guid fileId, CancellationToken ct = default);
    Task<IReadOnlyList<SysFileScanLogDto>> ListFileScanLogsAsync(Guid tenantId, Guid fileId, CancellationToken ct = default);
    Task EnsureFileDownloadAllowedAsync(Guid tenantId, Guid fileId, CancellationToken ct = default);

    // 077 bulk export
    Task<SysBulkExportJobDto> StartBulkExportAsync(Guid tenantId, Guid userId, SysBulkExportRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<SysBulkExportJobDto>> ListExportJobsAsync(Guid tenantId, int take, CancellationToken ct = default);
    Task<SysBulkExportDownloadDto> DownloadExportJobAsync(Guid tenantId, Guid jobId, CancellationToken ct = default);

    // 082 IP
    Task<IReadOnlyList<SysIpRuleDto>> ListIpRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task<SysIpRuleDto> UpsertIpRuleAsync(Guid tenantId, Guid userId, SysIpRuleUpsertRequest req, CancellationToken ct = default);
    Task DeleteIpRuleAsync(Guid tenantId, Guid ruleId, CancellationToken ct = default);
    Task<SysIpCheckResult> EvaluateIpAsync(Guid tenantId, string? ip, CancellationToken ct = default);
    Task EnsureIpAllowedAsync(Guid tenantId, string? ip, CancellationToken ct = default);
}
