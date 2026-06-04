using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.NotificationService
{
    public interface INotificationLogService
    {
        Task<PaginatedResult<NotificationLogResult>> GetNotificationsByUserAsync(string userId, NotificationLogFilter filter);
        Task<int> GetUnreadPushCountAsync(string userId);
        Task MarkPushNotificationReadAsync(Guid notificationId);
        Task MarkAllPushNotificationsReadAsync(string userId);
        Task<IEnumerable<UnreadPushNotificationResult>> GetUnreadPushNotificationsAsync(string userId);
    }
}
