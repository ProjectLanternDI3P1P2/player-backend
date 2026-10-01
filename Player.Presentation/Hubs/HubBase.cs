using Microsoft.AspNetCore.SignalR;

namespace Player.Presentation.Hubs;

/// <summary>Shared server-authoritative group operations for gameplay hubs.</summary>
public abstract class HubBase : Hub
{
    protected Task AddCallerToGroupAsync(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        return Groups.AddToGroupAsync(Context.ConnectionId, groupName, Context.ConnectionAborted);
    }

    protected Task BroadcastToGroupAsync<TMessage>(
        string groupName,
        string eventName,
        TMessage message
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        return Clients.Group(groupName).SendAsync(eventName, message, Context.ConnectionAborted);
    }
}
