using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.NotificationModule
{
    public class NotificationPreference : BaseEntity<int>
    {
        public string UserId { get; set; } = string.Empty!;
        public RecipientType RecipientType { get; set; }
        public NotificationType NotificationType { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
