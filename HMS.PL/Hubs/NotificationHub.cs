using HMS.PL.Senders;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HMS.PL.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                NotificationPushSender.TrackConnect(userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                NotificationPushSender.TrackDisconnect(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        public async Task LeaveUserGroup(string userId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
    }
}
