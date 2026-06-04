using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationTemplateSpecifications
{
    public sealed class PreferenceByUserTypeChannelSpec : BaseSpecifications<NotificationPreference, int>
    {
        public PreferenceByUserTypeChannelSpec(string userId, NotificationType type, NotificationChannel channel)
            : base(p =>
                p.UserId == userId &&
                p.NotificationType == type &&
                p.Channel == channel)
        { }
    }
}
