using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.NotificationModule;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Implementations.NotificationModule
{
    public sealed class AdminNotificationLogService(
        IUnitOfWork _unitOfWork,
        IMapper _mapper) : IAdminNotificationLogService
    {
        public async Task<PaginatedResult<NotificationLogResult>> GetAllNotificationsAsync(
            NotificationLogFilter filter)
        {
            var repo = _unitOfWork.GetRepository<Notification, Guid>();
            var notifications = await repo.GetAllAsync(new AdminNotificationLogSpec(filter));
            var count = await repo.CountAsync(new AdminNotificationLogCountSpec(filter));

            return new PaginatedResult<NotificationLogResult>(
                filter.PageIndex, filter.PageSize, count,
                _mapper.Map<IEnumerable<NotificationLogResult>>(notifications));
        }
    }
}
