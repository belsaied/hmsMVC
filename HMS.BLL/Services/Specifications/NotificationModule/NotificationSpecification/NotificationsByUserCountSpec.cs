using HMS.DAL.Models.NotificationModule;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification
{
    public sealed class NotificationsByUserCountSpec : BaseSpecifications<Notification, Guid>
    {
        public NotificationsByUserCountSpec(string userId, NotificationLogFilter filter)
            : base(n =>
                n.RecipientUserId == userId &&
                (!filter.Channel.HasValue || n.Channel == filter.Channel.Value) &&
                (!filter.NotificationType.HasValue || n.NotificationType == filter.NotificationType.Value) &&
                (!filter.DeliveryStatus.HasValue || n.DeliveryStatus == filter.DeliveryStatus.Value) &&
                (!filter.From.HasValue || n.CreatedAt >= filter.From.Value) &&
                (!filter.To.HasValue || n.CreatedAt <= filter.To.Value))
        { }
    }
}
