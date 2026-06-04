using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Results
{
    public record NotificationLogResult
    {
        public Guid Id { get; init; }
        public RecipientType RecipientType { get; init; }
        public NotificationChannel Channel { get; init; }
        public NotificationType NotificationType { get; init; }
        public string? Subject { get; init; }
        public string Body { get; init; } = string.Empty;
        public DeliveryStatus DeliveryStatus { get; init; }
        public string? FailureReason { get; init; }
        public string? RelatedEntityId { get; init; }
        public string? RelatedEntityType { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? SentAt { get; init; }
        public bool IsRead { get; init; }
    }
}
