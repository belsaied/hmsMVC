using HMS.DAL.Models.NotificationModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification
{
    public sealed class NotificationByIdSpec : BaseSpecifications<Notification, Guid>
    {
        public NotificationByIdSpec(Guid id) : base(n => n.Id == id) { }
    }
}
