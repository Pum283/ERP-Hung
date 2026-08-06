using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Erp.Api.Hubs;

/// <summary>Digi-style: join/leave + typing; REST fan-out tin nhắn qua user groups.</summary>
[Authorize]
public sealed class MsgHub : Hub
{
    public static string UserGroup(Guid userId) => $"user:{userId:D}";
    public static string ConvGroup(Guid conversationId) => $"conv:{conversationId:D}";

    public override async Task OnConnectedAsync()
    {
        var id = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? Context.User?.FindFirstValue("sub");
        if (Guid.TryParse(id, out var userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

        await base.OnConnectedAsync();
    }

    public Task JoinConversation(Guid conversationId)
        => Groups.AddToGroupAsync(Context.ConnectionId, ConvGroup(conversationId));

    public Task LeaveConversation(Guid conversationId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, ConvGroup(conversationId));

    public async Task SendTypingStatus(Guid conversationId, bool isTyping)
    {
        var id = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? Context.User?.FindFirstValue("sub");
        if (!Guid.TryParse(id, out var userId)) return;
        var name = Context.User?.FindFirstValue("name")
                   ?? Context.User?.FindFirstValue(ClaimTypes.Name)
                   ?? Context.User?.Identity?.Name
                   ?? "Someone";
        await Clients.OthersInGroup(ConvGroup(conversationId)).SendAsync(
            "ReceiveTypingStatus",
            new { conversationId, userId, fullName = name, isTyping });
    }
}
