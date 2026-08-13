using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Sys;

/// <summary>Bước 155 — Theme / Role home / Message search / Conversation mute.</summary>
[ApiController]
[Authorize]
[Route("api/sys")]
public sealed class SysThemeRoleHomeMsgController : ControllerBase
{
    private static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ISysThemeRoleHomeMsgService _svc;

    public SysThemeRoleHomeMsgController(ISysThemeRoleHomeMsgService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ── 093 ─────────────────────────────────────────────────────────────────

    [HttpGet("theme")]
    [AuthorizePermission("sys.brand.read")]
    public async Task<ActionResult<ApiResponse<SysThemeDto>>> GetTheme(CancellationToken ct)
        => Ok(ApiResponse<SysThemeDto>.Ok(await _svc.GetThemeAsync(TenantId, ct)));

    [HttpGet("theme/public")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<SysThemeDto>>> GetPublicTheme(
        [FromQuery] Guid? tenantId, CancellationToken ct)
        => Ok(ApiResponse<SysThemeDto>.Ok(await _svc.GetThemeAsync(tenantId ?? DefaultTenantId, ct)));

    [HttpPut("theme")]
    [AuthorizePermission("sys.brand.manage")]
    public async Task<ActionResult<ApiResponse<SysThemeDto>>> UpsertTheme(
        [FromBody] SysThemeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysThemeDto>.Ok(await _svc.UpsertThemeAsync(TenantId, UserId, req, ct)));

    // ── 094 ─────────────────────────────────────────────────────────────────

    [HttpGet("role-homes")]
    [AuthorizePermission("sys.ui.home.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysRoleHomeDto>>>> ListHomes(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SysRoleHomeDto>>.Ok(await _svc.ListRoleHomesAsync(TenantId, ct)));

    [HttpPut("role-homes")]
    [AuthorizePermission("sys.ui.home.manage")]
    public async Task<ActionResult<ApiResponse<SysRoleHomeDto>>> UpsertHome(
        [FromBody] SysRoleHomeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysRoleHomeDto>.Ok(await _svc.UpsertRoleHomeAsync(TenantId, UserId, req, ct)));

    [HttpDelete("role-homes/{id:guid}")]
    [AuthorizePermission("sys.ui.home.manage")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteHome(Guid id, CancellationToken ct)
    {
        await _svc.DeleteRoleHomeAsync(TenantId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("me/home")]
    public async Task<ActionResult<ApiResponse<SysMyHomeDto>>> MyHome(CancellationToken ct)
        => Ok(ApiResponse<SysMyHomeDto>.Ok(await _svc.ResolveMyHomeAsync(TenantId, UserId, ct)));

    // ── 103 ─────────────────────────────────────────────────────────────────

    [HttpGet("msg/messages/search")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SysMessageSearchHitDto>>>> SearchMessages(
        [FromQuery] string q,
        [FromQuery] Guid? conversationId,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SysMessageSearchHitDto>>.Ok(
            await _svc.SearchMessagesAsync(TenantId, UserId, q, conversationId, take, ct)));

    // ── 104 ─────────────────────────────────────────────────────────────────

    [HttpGet("msg/conversations/{id:guid}/mute")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<SysConversationMuteDto>>> GetMute(Guid id, CancellationToken ct)
        => Ok(ApiResponse<SysConversationMuteDto>.Ok(
            await _svc.GetConversationMuteAsync(TenantId, UserId, id, ct)));

    [HttpPut("msg/conversations/{id:guid}/mute")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<SysConversationMuteDto>>> SetMute(
        Guid id, [FromBody] SysConversationMuteRequest req, CancellationToken ct)
        => Ok(ApiResponse<SysConversationMuteDto>.Ok(
            await _svc.SetConversationMuteAsync(TenantId, UserId, id, req, ct)));
}
