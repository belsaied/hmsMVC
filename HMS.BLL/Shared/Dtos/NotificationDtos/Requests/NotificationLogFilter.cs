using HMS.DAL.Models.Enums.NotificationEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Requests
{
    public class NotificationLogFilter
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 20;

        public NotificationChannel? Channel { get; set; }
        public NotificationType? NotificationType { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int PageIndex { get; set; } = 1;

        private int _pageSize = DefaultPageSize;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
