using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Exceptions;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Sys;

[ApiController]
[Authorize]
[Route("api/sys")]
public sealed class SysPlatformController : ControllerBase
{
    private static readonly HashSet<string> LogoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/jpg", "image/webp", "image/svg+xml"
    };

    private const long LogoMaxBytes = 2 * 1024 * 1024; // 2 MB

    private readonly ISysPlatformService _svc;
    private readonly IFileStorageService _files;

    public SysPlatformController(ISysPlatformService svc, IFileStorageService files)
    {
        _svc = svc;
        _files = files;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("tenant")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<TenantDto>>> Tenant(CancellationToken ct)
        => Ok(ApiResponse<TenantDto>.Ok(await _svc.GetTenantAsync(TenantId, ct)));

    [HttpPut("tenant")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<TenantDto>>> UpdateTenant([FromBody] TenantUpdateRequest req, CancellationToken ct)
        => Ok(ApiResponse<TenantDto>.Ok(await _svc.UpdateTenantAsync(TenantId, req, ct)));

    /// <summary>
    /// Upload logo tenant → Cloudinary (hoặc local).
    /// Spec: PNG/JPEG/WebP/SVG · ≤ 2MB · khuyến nghị 512×512 (vuông), tối thiểu ~128px.
    /// </summary>
    [HttpPost("tenant/logo")]
    [AuthorizePermission("sys.license.manage")]
    [RequestSizeLimit(LogoMaxBytes + 64_000)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new AppException("Chưa chọn file logo.");
        if (file.Length > LogoMaxBytes)
            throw new AppException("Logo tối đa 2 MB.");
        var ctType = file.ContentType ?? "";
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var okExt = ext is ".png" or ".jpg" or ".jpeg" or ".webp" or ".svg";
        if (!LogoContentTypes.Contains(ctType) && !okExt)
            throw new AppException("Chỉ nhận PNG, JPEG, WebP hoặc SVG.");

        await using var stream = file.OpenReadStream();
        var saved = await _files.SaveAsync(stream, file.FileName, file.ContentType, TenantId, "brand", ct);
        var url = saved.PublicUrl
                  ?? (saved.StorageKey.StartsWith("cloudinary:", StringComparison.OrdinalIgnoreCase)
                      ? null
                      : $"/api/sys/files/{Uri.EscapeDataString(saved.StorageKey)}");
        if (string.IsNullOrWhiteSpace(url) && saved.StorageKey.StartsWith("cloudinary:", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Cloudinary không trả URL công khai.");

        var dto = await _svc.SetTenantLogoAsync(TenantId, url, saved.StorageKey, ct);
        return Ok(ApiResponse<TenantDto>.Ok(dto));
    }

    [HttpDelete("tenant/logo")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<TenantDto>>> ClearLogo(CancellationToken ct)
        => Ok(ApiResponse<TenantDto>.Ok(await _svc.SetTenantLogoAsync(TenantId, null, null, ct)));

    [HttpGet("modules")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ModuleCatalogItemDto>>>> Modules()
        => Ok(ApiResponse<IReadOnlyList<ModuleCatalogItemDto>>.Ok(await _svc.ListModulesAsync()));

    [HttpGet("licenses")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LicenseDto>>>> Licenses(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LicenseDto>>.Ok(await _svc.ListLicensesAsync(TenantId, ct)));

    [HttpPost("licenses")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<LicenseDto>>> UpsertLicense([FromBody] LicenseUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LicenseDto>.Ok(await _svc.UpsertLicenseAsync(TenantId, UserId, req, ct)));

    [HttpGet("license/status")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<LicenseStatusDto>>> LicenseStatus(CancellationToken ct)
        => Ok(ApiResponse<LicenseStatusDto>.Ok(await _svc.GetLicenseStatusAsync(TenantId, ct)));

    [HttpGet("password-policy")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<PasswordPolicyDto>>> GetPolicy(CancellationToken ct)
        => Ok(ApiResponse<PasswordPolicyDto>.Ok(await _svc.GetPasswordPolicyAsync(TenantId, ct)));

    [HttpPut("password-policy")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<PasswordPolicyDto>>> SetPolicy([FromBody] PasswordPolicyDto req, CancellationToken ct)
        => Ok(ApiResponse<PasswordPolicyDto>.Ok(await _svc.SetPasswordPolicyAsync(TenantId, req, ct)));

    [HttpGet("settings")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemSettingDto>>>> Settings(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SystemSettingDto>>.Ok(await _svc.ListSettingsAsync(TenantId, ct)));

    [HttpPut("settings/{key}")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<object>>> PutSetting(string key, [FromBody] SystemSettingDto body, CancellationToken ct)
    {
        await _svc.UpsertSettingAsync(TenantId, key, body.ValueJson, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("lookups/categories")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LookupCategoryDto>>>> LookupCats(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LookupCategoryDto>>.Ok(await _svc.ListLookupCategoriesAsync(TenantId, ct)));

    [HttpPost("lookups/categories")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<LookupCategoryDto>>> UpsertLookupCat([FromBody] LookupCategoryDto req, CancellationToken ct)
        => Ok(ApiResponse<LookupCategoryDto>.Ok(await _svc.UpsertLookupCategoryAsync(TenantId, UserId, req, ct)));

    [HttpGet("lookups/categories/{id:guid}/items")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LookupItemDto>>>> LookupItems(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LookupItemDto>>.Ok(await _svc.ListLookupItemsAsync(TenantId, id, ct)));

    [HttpPost("lookups/items")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<LookupItemDto>>> UpsertLookupItem([FromBody] LookupItemDto req, CancellationToken ct)
        => Ok(ApiResponse<LookupItemDto>.Ok(await _svc.UpsertLookupItemAsync(TenantId, UserId, req, ct)));

    [HttpGet("number-sequences")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NumberSequenceDto>>>> NumSeq(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<NumberSequenceDto>>.Ok(await _svc.ListNumberSequencesAsync(TenantId, ct)));

    [HttpPost("number-sequences")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<NumberSequenceDto>>> UpsertNumSeq([FromBody] NumberSequenceDto req, CancellationToken ct)
        => Ok(ApiResponse<NumberSequenceDto>.Ok(await _svc.UpsertNumberSequenceAsync(TenantId, UserId, req, ct)));

    [HttpPost("number-sequences/{docType}/next")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<object>>> NextNum(string docType, CancellationToken ct)
        => Ok(ApiResponse<object>.Ok(new { value = await _svc.NextNumberAsync(TenantId, docType, ct) }));

    [HttpGet("login-audits")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LoginAuditDto>>>> LoginAudits([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<LoginAuditDto>>.Ok(await _svc.ListLoginAuditsAsync(TenantId, take, ct)));

    [HttpGet("notifications")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AppNotificationDto>>>> Notifications(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AppNotificationDto>>.Ok(await _svc.ListNotificationsAsync(TenantId, UserId, ct)));

    [HttpGet("notifications/unread-count")]
    public async Task<ActionResult<ApiResponse<object>>> NotifUnread(CancellationToken ct)
        => Ok(ApiResponse<object>.Ok(new { count = await _svc.UnreadNotificationCountAsync(TenantId, UserId, ct) }));

    [HttpPost("notifications/{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> ReadNotif(Guid id, CancellationToken ct)
    {
        await _svc.MarkNotificationReadAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("notifications/read-all")]
    public async Task<ActionResult<ApiResponse<object>>> ReadAllNotifs(CancellationToken ct)
    {
        await _svc.MarkAllNotificationsReadAsync(TenantId, UserId, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("notification-rules")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationRuleDto>>>> Rules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<NotificationRuleDto>>.Ok(await _svc.ListNotificationRulesAsync(TenantId, ct)));

    [HttpPost("notification-rules")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<NotificationRuleDto>>> UpsertRule([FromBody] NotificationRuleDto req, CancellationToken ct)
        => Ok(ApiResponse<NotificationRuleDto>.Ok(await _svc.UpsertNotificationRuleAsync(TenantId, UserId, req, ct)));

    [HttpGet("export/users.csv")]
    [AuthorizePermission("sys.user.read")]
    public async Task<IActionResult> ExportUsers(CancellationToken ct)
    {
        var bytes = await _svc.ExportUsersCsvAsync(TenantId, UserId, ct);
        return File(bytes, "text/csv", "users.csv");
    }

    [HttpGet("import/templates/users.csv")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<IActionResult> ImportTemplate()
    {
        var t = await _svc.GetUserImportTemplateAsync();
        return File(Encoding.UTF8.GetBytes(t), "text/csv", "users_import_template.csv");
    }

    [HttpPost("import/users")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<object>>> ImportUsers(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var (ok, fail, errors) = await _svc.ImportUsersCsvAsync(TenantId, UserId, stream, ct);
        return Ok(ApiResponse<object>.Ok(new { ok, fail, errors }));
    }

    [HttpGet("legal-entities")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LegalEntityDto>>>> Legal(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LegalEntityDto>>.Ok(await _svc.ListLegalEntitiesAsync(TenantId, ct)));

    [HttpPost("legal-entities")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<LegalEntityDto>>> UpsertLegal([FromBody] LegalEntityDto req, CancellationToken ct)
        => Ok(ApiResponse<LegalEntityDto>.Ok(await _svc.UpsertLegalEntityAsync(TenantId, UserId, req, ct)));

    [HttpGet("sales-points")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SalesPointDto>>>> SalesPoints(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SalesPointDto>>.Ok(await _svc.ListSalesPointsAsync(TenantId, ct)));

    [HttpPost("sales-points")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<SalesPointDto>>> UpsertSales([FromBody] SalesPointDto req, CancellationToken ct)
        => Ok(ApiResponse<SalesPointDto>.Ok(await _svc.UpsertSalesPointAsync(TenantId, UserId, req, ct)));

    [HttpGet("provinces")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProvinceDto>>>> Provinces(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ProvinceDto>>.Ok(await _svc.ListProvincesAsync(TenantId, ct)));

    [HttpPost("provinces")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<ProvinceDto>>> UpsertProvince([FromBody] ProvinceDto req, CancellationToken ct)
        => Ok(ApiResponse<ProvinceDto>.Ok(await _svc.UpsertProvinceAsync(TenantId, UserId, req, ct)));

    [HttpGet("org-chart")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrgNodeDto>>>> OrgChart(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OrgNodeDto>>.Ok(await _svc.GetOrgChartAsync(TenantId, ct)));

    [HttpGet("work-calendars")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkCalendarDto>>>> Calendars(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WorkCalendarDto>>.Ok(await _svc.ListWorkCalendarsAsync(TenantId, ct)));

    [HttpPost("work-calendars")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<WorkCalendarDto>>> UpsertCalendar([FromBody] WorkCalendarDto req, CancellationToken ct)
        => Ok(ApiResponse<WorkCalendarDto>.Ok(await _svc.UpsertWorkCalendarAsync(TenantId, UserId, req, ct)));

    [HttpGet("message-templates")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MessageTemplateDto>>>> Templates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MessageTemplateDto>>.Ok(await _svc.ListMessageTemplatesAsync(TenantId, ct)));

    [HttpPost("message-templates")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<MessageTemplateDto>>> UpsertTemplate([FromBody] MessageTemplateDto req, CancellationToken ct)
        => Ok(ApiResponse<MessageTemplateDto>.Ok(await _svc.UpsertMessageTemplateAsync(TenantId, UserId, req, ct)));

    [HttpGet("file-folders")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FileFolderDto>>>> Folders(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FileFolderDto>>.Ok(await _svc.ListFoldersAsync(TenantId, ct)));

    [HttpPost("file-folders")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<FileFolderDto>>> UpsertFolder([FromBody] FileFolderDto req, CancellationToken ct)
        => Ok(ApiResponse<FileFolderDto>.Ok(await _svc.UpsertFolderAsync(TenantId, UserId, req, ct)));

    [HttpGet("files")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FileObjectDto>>>> Files([FromQuery] Guid? folderId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FileObjectDto>>.Ok(await _svc.ListFilesAsync(TenantId, folderId, ct)));

    [HttpPost("files/{id:guid}/soft-delete")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<object>>> SoftDeleteFile(Guid id, CancellationToken ct)
    {
        await _svc.SoftDeleteFileAsync(TenantId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("files/{id:guid}/restore")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<object>>> RestoreFile(Guid id, CancellationToken ct)
    {
        await _svc.RestoreFileAsync(TenantId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("api-keys")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApiKeyDto>>>> ApiKeys(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ApiKeyDto>>.Ok(await _svc.ListApiKeysAsync(TenantId, ct)));

    [HttpPost("api-keys")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<ApiKeyCreatedDto>>> CreateApiKey([FromBody] ApiKeyDto req, CancellationToken ct)
        => Ok(ApiResponse<ApiKeyCreatedDto>.Ok(await _svc.CreateApiKeyAsync(TenantId, UserId, req.Name, ct)));

    [HttpDelete("api-keys/{id:guid}")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeKey(Guid id, CancellationToken ct)
    {
        await _svc.RevokeApiKeyAsync(TenantId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("webhooks")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WebhookDto>>>> Webhooks(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WebhookDto>>.Ok(await _svc.ListWebhooksAsync(TenantId, ct)));

    [HttpPost("webhooks")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<WebhookDto>>> UpsertWebhook([FromBody] WebhookDto req, CancellationToken ct)
        => Ok(ApiResponse<WebhookDto>.Ok(await _svc.UpsertWebhookAsync(TenantId, UserId, req, ct)));

    [HttpGet("integration-logs")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IntegrationLogDto>>>> IntLogs([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<IntegrationLogDto>>.Ok(await _svc.ListIntegrationLogsAsync(TenantId, take, ct)));

    [HttpGet("integrations")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExternalIntegrationDto>>>> Integrations(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ExternalIntegrationDto>>.Ok(await _svc.ListIntegrationsAsync(TenantId, ct)));

    [HttpPost("integrations")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<ExternalIntegrationDto>>> UpsertIntegration([FromBody] ExternalIntegrationDto req, CancellationToken ct)
        => Ok(ApiResponse<ExternalIntegrationDto>.Ok(await _svc.UpsertIntegrationAsync(TenantId, UserId, req, ct)));

    /// <summary>UC_SYS_060/061 — gửi thử Email/SMS stub (ghi IntegrationCallLog).</summary>
    [HttpPost("integrations/send")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<ChannelSendResultDto>>> SendChannel(
        [FromBody] ChannelSendRequest req, CancellationToken ct)
        => Ok(ApiResponse<ChannelSendResultDto>.Ok(await _svc.SendChannelMessageAsync(TenantId, UserId, req, ct)));

    [HttpGet("locales")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LocalePackDto>>>> Locales(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LocalePackDto>>.Ok(await _svc.ListLocalePacksAsync(TenantId, ct)));

    [HttpPut("me/locale")]
    public async Task<ActionResult<ApiResponse<object>>> SetLocale([FromBody] SetLocaleRequest req, CancellationToken ct)
    {
        await _svc.SetUserLocaleAsync(TenantId, UserId, req.LocaleCode, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("permission-matrix")]
    [AuthorizePermission("sys.role.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleMatrixRowDto>>>> Matrix(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RoleMatrixRowDto>>.Ok(await _svc.GetPermissionMatrixAsync(TenantId, ct)));

    [HttpGet("permission-change-logs")]
    [AuthorizePermission("sys.role.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissionChangeLogDto>>>> PermLogs([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PermissionChangeLogDto>>.Ok(await _svc.ListPermissionChangeLogsAsync(TenantId, take, ct)));
}
