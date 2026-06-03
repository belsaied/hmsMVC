using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.NotificationModule
{
    public class NotificationTemplate : BaseEntity<int>
    {
        public NotificationType NotificationType { get; set; }
        public NotificationChannel Channel { get; set; }
        public string? SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
