using AutoMapper;
using HMS.DAL.Models.NotificationModule;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.NotificationModule
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationLogResult>();

            CreateMap<Notification, UnreadPushNotificationResult>();

            CreateMap<NotificationPreference, NotificationPreferenceResult>();
        }
    }
}
