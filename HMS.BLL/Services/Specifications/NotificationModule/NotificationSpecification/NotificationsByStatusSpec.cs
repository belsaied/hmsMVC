using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;

namespace HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification
{
    public sealed class NotificationsByStatusSpec : BaseSpecifications<Notification, Guid>
    {
        public NotificationsByStatusSpec(string userId, DeliveryStatus status)
            : base(n => n.RecipientUserId == userId && n.DeliveryStatus == status)
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}
