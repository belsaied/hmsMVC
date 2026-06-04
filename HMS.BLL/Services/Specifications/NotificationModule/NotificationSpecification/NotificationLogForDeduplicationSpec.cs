using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification
{
    public sealed class NotificationLogForDeduplicationSpec : BaseSpecifications<Notification, Guid>
    {
        public NotificationLogForDeduplicationSpec(
            string relatedEntityId,
            NotificationType notificationType,
            int lookBackDays)
            : base(n =>
                n.RelatedEntityId == relatedEntityId &&
                n.NotificationType == notificationType &&
                n.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-lookBackDays))
        { }
    }
}
