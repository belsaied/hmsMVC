using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationTemplateSpecifications
{
    public sealed class ActiveTemplateSpec : BaseSpecifications<NotificationTemplate, int>
    {
        public ActiveTemplateSpec(NotificationType type, NotificationChannel channel)
            : base(t =>
                t.NotificationType == type &&
                t.Channel == channel &&
                t.IsActive)
        { }
    }
}
