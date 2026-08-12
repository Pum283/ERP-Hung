using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Sys;

/// <summary>Bước 154 — Notif prefs / File scan / Bulk export / IP rules.</summary>
[ApiController]
[Authorize]
[Route("api/sys")]
public sealed class SysStep154Controller : ControllerBase
{
    private readonly ISysStep154Service _svc;

    public SysStep154Controller(ISysStep154Service svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ── 064 ─────────────────────────────────────────────────────────────────

    [HttpGet("me/notification-preferences")]
    public async Task<ActionResult<ApiResponse<SysNotificationPreferenceDto>>> GetPrefs(CancellationToken ct)
        => Ok(ApiResponse<SysNotificationPreferenceDto>.Ok(
            await _svc.GetMyNotificationPreferencesAsync(TenantId, UserId, ct)));

    [HttpPut("me/notification-preferences")]
    public async Task<ActionResult<ApiResponse<SysNotificationPreferenceDto>>> UpsertPrefs(
        [FromBody] SysNotificationPreferenceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysNotificationPreferenceDto>.Ok(
            await _svc.UpsertMyNotificationPreferencesAsync(TenantId, UserId, req, ct)));

    // ── 071 ─────────────────────────────────────────────────────────────────

    [HttpPost("files/{id:guid}/scan")]
    [AuthorizePermission("sys.file.scan")]
    public async Task<ActionResult<ApiResponse<SysFileScanStatusDto>>> ScanFile(
        Guid id, [FromBody] ScanHintBody? body, CancellationToken ct)
        => Ok(ApiResponse<SysFileScanStatusDto>.Ok(
            await _svc.ScanFileAsync(TenantId, UserId, id, body?.ContentHint, ct)));

    [HttpGet("files/{id:guid}/scan-status")]
    [AuthorizePermission("sys.file.scan")]
    public async Task<ActionResult<ApiResponse<SysFileScanStatusDto>>> ScanStatus(Guid id, CancellationToken ct)
        => Ok(ApiResponse<SysFileScanStatusDto>.Ok(await _svc.GetFileScanStatusAsync(TenantId, id, ct)));

    [HttpGet("files/{id:guid}/scan-logs")]
    [AuthorizePermission("sys.file.scan")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysFileScanLogDto>>>> ScanLogs(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysFileScanLogDto>>.Ok(await _svc.ListFileScanLogsAsync(TenantId, id, ct)));

    public sealed record ScanHintBody(string? ContentHint);

    // ── 077 ─────────────────────────────────────────────────────────────────

    [HttpPost("export/bulk")]
    [AuthorizePermission("sys.export.bulk")]
    public async Task<ActionResult<ApiResponse<SysBulkExportJobDto>>> BulkExport(
        [FromBody] SysBulkExportRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysBulkExportJobDto>.Ok(await _svc.StartBulkExportAsync(TenantId, UserId, req, ct)));

    [HttpGet("export/jobs")]
    [AuthorizePermission("sys.export.job.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysBulkExportJobDto>>>> ExportJobs(
        [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SysBulkExportJobDto>>.Ok(await _svc.ListExportJobsAsync(TenantId, take, ct)));

    [HttpGet("export/jobs/{id:guid}/download")]
    [AuthorizePermission("sys.export.job.read")]
    public async Task<IActionResult> DownloadJob(Guid id, CancellationToken ct)
    {
        var file = await _svc.DownloadExportJobAsync(TenantId, id, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    // ── 082 ─────────────────────────────────────────────────────────────────

    [HttpGet("ip-rules")]
    [AuthorizePermission("sys.ip.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysIpRuleDto>>>> ListIp(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysIpRuleDto>>.Ok(await _svc.ListIpRulesAsync(TenantId, ct)));

    [HttpPut("ip-rules")]
    [AuthorizePermission("sys.ip.manage")]
    public async Task<ActionResult<ApiResponse<SysIpRuleDto>>> UpsertIp(
        [FromBody] SysIpRuleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysIpRuleDto>.Ok(await _svc.UpsertIpRuleAsync(TenantId, UserId, req, ct)));

    [HttpDelete("ip-rules/{id:guid}")]
    [AuthorizePermission("sys.ip.manage")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteIp(Guid id, CancellationToken ct)
    {
        await _svc.DeleteIpRuleAsync(TenantId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("ip-rules/check")]
    [AuthorizePermission("sys.ip.read")]
    public async Task<ActionResult<ApiResponse<SysIpCheckResult>>> CheckIp(
        [FromBody] IpCheckBody body, CancellationToken ct)
        => Ok(ApiResponse<SysIpCheckResult>.Ok(await _svc.EvaluateIpAsync(TenantId, body.Ip, ct)));

    public sealed record IpCheckBody(string? Ip);
}
