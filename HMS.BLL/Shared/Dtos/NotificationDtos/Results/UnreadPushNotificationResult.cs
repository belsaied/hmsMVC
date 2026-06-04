using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Results
{
    public record UnreadPushNotificationResult
    {
        public Guid Id { get; init; }
        public NotificationType NotificationType { get; init; }
        public string? Subject { get; init; }
        public string Body { get; init; } = string.Empty;
        public string? RelatedEntityId { get; init; }
        public string? RelatedEntityType { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
