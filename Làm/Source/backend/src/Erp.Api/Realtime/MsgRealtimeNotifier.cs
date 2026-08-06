using Erp.Api.Hubs;
using Erp.Application.Interfaces.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Erp.Api.Realtime;

public sealed class MsgRealtimeNotifier : IMsgRealtimeNotifier
{
    private readonly IHubContext<MsgHub> _hub;

    public MsgRealtimeNotifier(IHubContext<MsgHub> hub) => _hub = hub;

    public async Task MessageReceivedAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default)
    {
        foreach (var uid in userIds.Distinct())
            await _hub.Clients.Group(MsgHub.UserGroup(uid)).SendAsync("messageReceived", payload, ct);
    }

    public async Task MessageEditedAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default)
    {
        foreach (var uid in userIds.Distinct())
            await _hub.Clients.Group(MsgHub.UserGroup(uid)).SendAsync("messageEdited", payload, ct);
    }

    public async Task ReactionToggledAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default)
    {
        foreach (var uid in userIds.Distinct())
            await _hub.Clients.Group(MsgHub.UserGroup(uid)).SendAsync("reactionToggled", payload, ct);
    }

    public async Task ConversationUpdatedAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default)
    {
        foreach (var uid in userIds.Distinct())
            await _hub.Clients.Group(MsgHub.UserGroup(uid)).SendAsync("conversationUpdated", payload, ct);
    }

    public Task TypingAsync(Guid conversationId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(MsgHub.ConvGroup(conversationId)).SendAsync("ReceiveTypingStatus", payload, ct);
}
