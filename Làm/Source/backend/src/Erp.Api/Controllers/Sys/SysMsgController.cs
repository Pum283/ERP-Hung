using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Sys;

[ApiController]
[Authorize]
[Route("api/sys/msg")]
public sealed class SysMsgController : ControllerBase
{
    private readonly IMsgService _svc;

    public SysMsgController(IMsgService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("conversations")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConversationDto>>>> Conversations(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ConversationDto>>.Ok(await _svc.ListConversationsAsync(TenantId, UserId, ct)));

    [HttpPost("conversations")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> Create([FromBody] CreateConversationRequest req, CancellationToken ct)
        => Ok(ApiResponse<ConversationDto>.Ok(await _svc.CreateConversationAsync(TenantId, UserId, req, ct)));

    [HttpGet("conversations/{id:guid}/messages")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatMessageDto>>>> Messages(
        Guid id, [FromQuery] DateTimeOffset? before, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ChatMessageDto>>.Ok(
            await _svc.ListMessagesAsync(TenantId, UserId, id, before, take, ct)));

    [HttpPost("conversations/{id:guid}/messages")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> Send(Guid id, [FromBody] SendMessageRequest req, CancellationToken ct)
        => Ok(ApiResponse<ChatMessageDto>.Ok(await _svc.SendMessageAsync(TenantId, UserId, id, req, ct)));

    [HttpPut("conversations/{id:guid}/messages/{messageId:guid}")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> Edit(
        Guid id, Guid messageId, [FromBody] EditMessageRequest req, CancellationToken ct)
        => Ok(ApiResponse<ChatMessageDto>.Ok(await _svc.EditMessageAsync(TenantId, UserId, id, messageId, req, ct)));

    [HttpPost("conversations/{id:guid}/messages/{messageId:guid}/recall")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> Recall(Guid id, Guid messageId, CancellationToken ct)
        => Ok(ApiResponse<ChatMessageDto>.Ok(await _svc.RecallMessageAsync(TenantId, UserId, id, messageId, ct)));

    [HttpPost("conversations/{id:guid}/messages/{messageId:guid}/reactions")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<ReactionToggledDto>>> ToggleReaction(
        Guid id, Guid messageId, [FromBody] ToggleReactionRequest req, CancellationToken ct)
        => Ok(ApiResponse<ReactionToggledDto>.Ok(
            await _svc.ToggleReactionAsync(TenantId, UserId, id, messageId, req, ct)));

    [HttpPost("conversations/{id:guid}/read")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<object>>> Read(Guid id, CancellationToken ct)
    {
        await _svc.MarkReadAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("conversations/{id:guid}/mute")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<object>>> Mute(Guid id, [FromBody] MuteConversationRequest req, CancellationToken ct)
    {
        await _svc.SetMutedAsync(TenantId, UserId, id, req.Muted, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("conversations/{id:guid}/members")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConversationMemberDto>>>> Members(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ConversationMemberDto>>.Ok(await _svc.ListMembersAsync(TenantId, UserId, id, ct)));

    [HttpPost("conversations/{id:guid}/members")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<object>>> AddMembers(Guid id, [FromBody] AddMembersRequest req, CancellationToken ct)
    {
        await _svc.AddMembersAsync(TenantId, UserId, id, req.MemberIds, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpDelete("conversations/{id:guid}/members/{memberUserId:guid}")]
    [AuthorizePermission("sys.msg.send")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveMember(Guid id, Guid memberUserId, CancellationToken ct)
    {
        await _svc.RemoveMemberAsync(TenantId, UserId, id, memberUserId, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("unread-count")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<UnreadCountDto>>> Unread(CancellationToken ct)
        => Ok(ApiResponse<UnreadCountDto>.Ok(await _svc.GetUnreadCountAsync(TenantId, UserId, ct)));

    [HttpGet("directory")]
    [AuthorizePermission("sys.msg.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MsgDirectoryUserDto>>>> Directory(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MsgDirectoryUserDto>>.Ok(await _svc.ListDirectoryAsync(TenantId, UserId, ct)));
}
