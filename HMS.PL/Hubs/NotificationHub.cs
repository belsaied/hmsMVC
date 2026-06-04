using Microsoft.AspNetCore.SignalR;

namespace HMS.PL.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        public async Task LeaveUserGroup(string userId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
    }
}
