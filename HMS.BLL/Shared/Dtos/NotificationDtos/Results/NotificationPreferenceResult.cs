using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Results
{
    public record NotificationPreferenceResult
    {
        public NotificationType NotificationType { get; init; }
        public NotificationChannel Channel { get; init; }
        public bool IsEnabled { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}
