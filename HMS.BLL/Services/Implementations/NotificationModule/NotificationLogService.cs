using AutoMapper;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;
using HMS.DAL.Contracts;
using HMS.DAL.Models.NotificationModule;

namespace HMS.BLL.Services.Implementations.NotificationModule
{
    public sealed class NotificationLogService(
         IUnitOfWork _unitOfWork,
         IMapper _mapper) : INotificationLogService
    {
        public async Task<PaginatedResult<NotificationLogResult>> GetNotificationsByUserAsync(
            string userId, NotificationLogFilter filter)
        {
            var repo = _unitOfWork.GetRepository<Notification, Guid>();
            var notifications = await repo.GetAllAsync(new NotificationsByUserSpec(userId, filter));
            var count = await repo.CountAsync(new NotificationsByUserCountSpec(userId, filter));
            return new PaginatedResult<NotificationLogResult>(
                filter.PageIndex, filter.PageSize, count,
                _mapper.Map<IEnumerable<NotificationLogResult>>(notifications));
        }

        public async Task<IEnumerable<UnreadPushNotificationResult>> GetUnreadPushNotificationsAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<Notification, Guid>();
            var unread = await repo.GetAllAsync(new UnreadPushNotificationsByUserSpec(userId));
            return _mapper.Map<IEnumerable<UnreadPushNotificationResult>>(unread);
        }

        public async Task<int> GetUnreadPushCountAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<Notification, Guid>();
            return await repo.CountAsync(new UnreadPushNotificationsByUserSpec(userId));
        }

        public async Task MarkPushNotificationReadAsync(Guid notificationId)
        {
            var repo = _unitOfWork.GetRepository<Notification, Guid>();
            var notification = await repo.GetByIdAsync(new NotificationByIdSpec(notificationId))
                ?? throw new NotificationNotFoundException(notificationId);

            notification.IsRead = true;
            repo.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllPushNotificationsReadAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<Notification, Guid>();
            var unread = (await repo.GetAllAsync(new UnreadPushNotificationsByUserSpec(userId))).ToList();

            foreach (var n in unread)
            {
                n.IsRead = true;
                repo.Update(n);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
