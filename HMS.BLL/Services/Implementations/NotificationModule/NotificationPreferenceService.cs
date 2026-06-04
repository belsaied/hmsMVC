using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.NotificationModule;
using Services.Abstraction.Contracts.NotificationService;
using HMS.BLL.Services.Specifications.NotificationModule;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationTemplateSpecifications;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.BLL.Shared.Dtos.NotificationDtos.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Implementations.NotificationModule
{
    public sealed class NotificationPreferenceService(
        IUnitOfWork _unitOfWork,
        IMapper _mapper) : INotificationPreferenceService
    {
        public async Task<IEnumerable<NotificationPreferenceResult>> GetPreferencesAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<NotificationPreference, int>();
            var preferences = await repo.GetAllAsync(new PreferencesByUserSpec(userId));
            return _mapper.Map<IEnumerable<NotificationPreferenceResult>>(preferences);
        }

        public async Task<NotificationPreferenceResult> UpdatePreferenceAsync(string userId, UpdatePreferenceRequest request)
        {
            var repo = _unitOfWork.GetRepository<NotificationPreference, int>();
            var existing = await repo.GetByIdAsync(
                new PreferenceByUserTypeChannelSpec(userId, request.NotificationType, request.Channel));

            if (existing is not null)
            {
                existing.IsEnabled = request.IsEnabled;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                repo.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<NotificationPreferenceResult>(existing);
            }

            // Create new preference row
            var preference = new NotificationPreference
            {
                UserId = userId,
                NotificationType = request.NotificationType,
                Channel = request.Channel,
                IsEnabled = request.IsEnabled,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await repo.AddAsync(preference);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<NotificationPreferenceResult>(preference);
        }

        public async Task ResetPreferencesToDefaultAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<NotificationPreference, int>();
            var preferences = (await repo.GetAllAsync(new PreferencesByUserSpec(userId))).ToList();

            foreach (var pref in preferences)
            {
                pref.IsEnabled = true;
                pref.UpdatedAt = DateTimeOffset.UtcNow;
                repo.Update(pref);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
