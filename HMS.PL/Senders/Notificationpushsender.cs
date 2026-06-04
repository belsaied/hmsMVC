using Microsoft.AspNetCore.SignalR;
using HMS.PL.Hubs;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;

namespace HMS.PL.Senders
{
    public sealed class NotificationPushSender(IHubContext<NotificationHub> _hubContext) : INotificationPushSender
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, int>
            _connectionCounts = new();

        public async Task SendAsync(string userId, object payload)
            => await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync("ReceiveNotification", payload);

        public Task<bool> IsUserConnectedAsync(string userId)
            => Task.FromResult(_connectionCounts.TryGetValue(userId, out var count) && count > 0);

        public static void TrackConnect(string userId)
            => _connectionCounts.AddOrUpdate(userId, 1, (_, c) => c + 1);

        public static void TrackDisconnect(string userId)
            => _connectionCounts.AddOrUpdate(userId, 0, (_, c) => Math.Max(0, c - 1));
    }
}