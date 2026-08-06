using Erp.Api.Hubs;
using Erp.Application.Interfaces.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Erp.Api.Realtime;

public sealed class WfRealtimeNotifier : IWfRealtimeNotifier
{
    private readonly IHubContext<WfHub> _hub;

    public WfRealtimeNotifier(IHubContext<WfHub> hub) => _hub = hub;

    public Task NotifyInboxChangedAsync(Guid userId, string reason, Guid? taskId = null, CancellationToken ct = default)
        => _hub.Clients.Group(WfHub.UserGroup(userId)).SendAsync(
            "inboxChanged",
            new { reason, taskId },
            ct);
}