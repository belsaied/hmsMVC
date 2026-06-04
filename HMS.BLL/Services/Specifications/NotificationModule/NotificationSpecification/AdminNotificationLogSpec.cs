using HMS.DAL.Models.NotificationModule;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification
{
    public sealed class AdminNotificationLogSpec : BaseSpecifications<Notification, Guid>
    {
        public AdminNotificationLogSpec(NotificationLogFilter filter)
            : base(n =>
                (!filter.Channel.HasValue || n.Channel == filter.Channel.Value) &&
                (!filter.NotificationType.HasValue || n.NotificationType == filter.NotificationType.Value) &&
                (!filter.DeliveryStatus.HasValue || n.DeliveryStatus == filter.DeliveryStatus.Value) &&
                (!filter.From.HasValue || n.CreatedAt >= filter.From.Value) &&
                (!filter.To.HasValue || n.CreatedAt <= filter.To.Value))
        {
            AddOrderByDescending(n => n.CreatedAt);
            ApplyPagination(filter.PageSize, filter.PageIndex);
        }
    }
}
