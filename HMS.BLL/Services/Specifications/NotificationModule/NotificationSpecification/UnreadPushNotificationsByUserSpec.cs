using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification
{
    public sealed class UnreadPushNotificationsByUserSpec : BaseSpecifications<Notification, Guid>
    {
        public UnreadPushNotificationsByUserSpec(string userId)
            : base(n =>
                n.RecipientUserId == userId &&
                n.Channel == NotificationChannel.Push &&
                n.IsRead == false)
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}
