using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;

namespace HMS.PL.ViewModels.NotificationModule
{
    public class AdminNotificationLogViewModel
    {
        public required PaginatedResult<NotificationLogResult> PaginatedResult { get; init; }
        public NotificationLogFilter Filter { get; init; } = new();
    }
}
