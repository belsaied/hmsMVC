using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.NotificationService
{
    public interface IAdminNotificationLogService
    {
        Task<PaginatedResult<NotificationLogResult>> GetAllNotificationsAsync(NotificationLogFilter filter);
    }
}
