using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BalochiAcademy.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        if (Context.User?.FindFirst(ClaimTypes.Role)?.Value == "admin")
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

        await base.OnConnectedAsync();
    }

    public static async Task SendToUser(IHubContext<NotificationHub> hub, int userId, object payload)
        => await hub.Clients.Group($"user_{userId}").SendAsync("notification", payload);

    public static async Task SendToAdmins(IHubContext<NotificationHub> hub, object payload)
        => await hub.Clients.Group("admins").SendAsync("notification", payload);

    public static async Task Broadcast(IHubContext<NotificationHub> hub, object payload)
        => await hub.Clients.All.SendAsync("notification", payload);
}
