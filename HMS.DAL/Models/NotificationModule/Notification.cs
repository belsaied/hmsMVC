using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.NotificationModule
{
    public class Notification : BaseEntity<Guid>
    {
        public string RecipientUserId { get; set; } = string.Empty!;
        public RecipientType RecipientType { get; set; }
        public string? RecipientEmail { get; set; }   
        public string? RecipientPhone { get; set; }   
        public NotificationChannel Channel { get; set; }
        public NotificationType NotificationType { get; set; }
        public string? Subject { get; set; }   
        public string Body { get; set; } = null!;
        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
        public string? FailureReason { get; set; }
        public string? ExternalMessageId { get; set; }  
        public string? RelatedEntityId { get; set; }  
        public string? RelatedEntityType { get; set; } 
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? SentAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
