using Microsoft.AspNetCore.SignalR;

namespace Player.Presentation.Hubs;

/// <summary>
/// Provides the shared server-authoritative group workflow for application hubs.
/// Derived hubs choose the group and event names, while this hub ensures the caller
/// joins the group before its members receive the event.
/// </summary>
public abstract class HubBase : Hub
{
    protected async Task AddCallerToGroupAndBroadcastAsync<TMessage>(
        string groupName,
        string eventName,
        TMessage message
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName, Context.ConnectionAborted);
        await Clients.Group(groupName).SendAsync(eventName, message, Context.ConnectionAborted);
    }
}
