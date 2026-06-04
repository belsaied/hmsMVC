using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Requests
{
    public record UpdatePreferenceRequest
    {
        [Required]
        public NotificationType NotificationType { get; init; }

        [Required]
        public NotificationChannel Channel { get; init; }

        [Required]
        public bool IsEnabled { get; init; }
    }
}
