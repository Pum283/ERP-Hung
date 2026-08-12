using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Sys;

/// <summary>Bước 153 — SSO / Field ACL / Config versions / Push devices.</summary>
[ApiController]
[Authorize]
[Route("api/sys")]
public sealed class SysStep153Controller : ControllerBase
{
    private static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ISysStep153Service _svc;

    public SysStep153Controller(ISysStep153Service svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ── UC_SYS_009 SSO ──────────────────────────────────────────────────────

    [HttpGet("sso/providers")]
    [AuthorizePermission("sys.sso.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysSsoProviderDto>>>> ListSso(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysSsoProviderDto>>.Ok(await _svc.ListSsoProvidersAsync(TenantId, ct)));

    [HttpGet("sso/providers/public")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysSsoProviderPublicDto>>>> ListPublicSso(
        [FromQuery] Guid? tenantId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysSsoProviderPublicDto>>.Ok(
            await _svc.ListPublicSsoProvidersAsync(tenantId ?? DefaultTenantId, ct)));

    [HttpPut("sso/providers")]
    [AuthorizePermission("sys.sso.manage")]
    public async Task<ActionResult<ApiResponse<SysSsoProviderDto>>> UpsertSso(
        [FromBody] SysSsoProviderUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysSsoProviderDto>.Ok(await _svc.UpsertSsoProviderAsync(TenantId, UserId, req, ct)));

    [HttpPost("sso/start/{providerCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<SysSsoStartResponse>>> StartSso(
        string providerCode, [FromQuery] Guid? tenantId, CancellationToken ct)
        => Ok(ApiResponse<SysSsoStartResponse>.Ok(
            await _svc.StartSsoAsync(tenantId ?? DefaultTenantId, providerCode, ct)));

    [HttpPost("sso/callback")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> SsoCallback(
        [FromBody] SysSsoCallbackRequest req, [FromQuery] Guid? tenantId, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var result = await _svc.CompleteSsoAsync(tenantId ?? DefaultTenantId, req, ip, ua, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    // ── UC_SYS_031 Field permissions ────────────────────────────────────────

    [HttpGet("sensitive-fields")]
    [AuthorizePermission("sys.fieldperm.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysSensitiveFieldDto>>>> ListFields(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysSensitiveFieldDto>>.Ok(await _svc.ListSensitiveFieldsAsync(TenantId, ct)));

    [HttpPut("sensitive-fields")]
    [AuthorizePermission("sys.fieldperm.manage")]
    public async Task<ActionResult<ApiResponse<SysSensitiveFieldDto>>> UpsertField(
        [FromBody] SysSensitiveFieldUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysSensitiveFieldDto>.Ok(await _svc.UpsertSensitiveFieldAsync(TenantId, UserId, req, ct)));

    [HttpGet("roles/{roleId:guid}/field-permissions")]
    [AuthorizePermission("sys.fieldperm.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysRoleFieldPermissionDto>>>> ListRoleFieldPerms(
        Guid roleId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysRoleFieldPermissionDto>>.Ok(
            await _svc.ListRoleFieldPermissionsAsync(TenantId, roleId, ct)));

    [HttpPut("role-field-permissions")]
    [AuthorizePermission("sys.fieldperm.manage")]
    public async Task<ActionResult<ApiResponse<SysRoleFieldPermissionDto>>> UpsertRoleFieldPerm(
        [FromBody] SysRoleFieldPermissionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysRoleFieldPermissionDto>.Ok(
            await _svc.UpsertRoleFieldPermissionAsync(TenantId, UserId, req, ct)));

    [HttpGet("me/field-access")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysEffectiveFieldAccessDto>>>> MyFieldAccess(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysEffectiveFieldAccessDto>>.Ok(
            await _svc.GetMyFieldAccessAsync(TenantId, UserId, ct)));

    // ── UC_SYS_058 Config versions ──────────────────────────────────────────

    [HttpPut("settings/{key}/versioned")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<object>>> UpsertVersioned(
        string key, [FromBody] SysConfigUpsertVersionedRequest body, CancellationToken ct)
    {
        var req = body with { Key = key };
        await _svc.UpsertSettingVersionedAsync(TenantId, UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("settings/{key}/versions")]
    [AuthorizePermission("sys.config.version.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysConfigVersionDto>>>> ListVersions(
        string key, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysConfigVersionDto>>.Ok(
            await _svc.ListConfigVersionsAsync(TenantId, key, ct)));

    [HttpPost("settings/{key}/rollback")]
    [AuthorizePermission("sys.config.version.rollback")]
    public async Task<ActionResult<ApiResponse<SysConfigVersionDto>>> Rollback(
        string key, [FromBody] SysConfigRollbackRequest body, CancellationToken ct)
    {
        var req = body with { Key = key };
        return Ok(ApiResponse<SysConfigVersionDto>.Ok(await _svc.RollbackConfigAsync(TenantId, UserId, req, ct)));
    }

    // ── UC_SYS_062 Push ─────────────────────────────────────────────────────

    [HttpGet("push/devices")]
    [AuthorizePermission("sys.push.device.self")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysPushDeviceDto>>>> MyDevices(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysPushDeviceDto>>.Ok(
            await _svc.ListMyPushDevicesAsync(TenantId, UserId, ct)));

    [HttpPost("push/devices")]
    [AuthorizePermission("sys.push.device.self")]
    public async Task<ActionResult<ApiResponse<SysPushDeviceDto>>> RegisterDevice(
        [FromBody] SysPushDeviceRegisterRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysPushDeviceDto>.Ok(
            await _svc.RegisterPushDeviceAsync(TenantId, UserId, req, ct)));

    [HttpDelete("push/devices/{id:guid}")]
    [AuthorizePermission("sys.push.device.self")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeDevice(Guid id, CancellationToken ct)
    {
        await _svc.RevokePushDeviceAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("push/test")]
    [AuthorizePermission("sys.push.manage")]
    public async Task<ActionResult<ApiResponse<SysPushSendResult>>> TestPush(
        [FromBody] SysPushTestRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysPushSendResult>.Ok(await _svc.SendTestPushAsync(TenantId, UserId, req, ct)));
}
