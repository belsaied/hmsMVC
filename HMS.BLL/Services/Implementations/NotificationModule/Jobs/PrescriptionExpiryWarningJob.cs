using HMS.DAL.Contracts;
using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.DAL.Models.NotificationModule;
using Microsoft.Extensions.Logging;
using Services.Abstraction.Contracts.NotificationService;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Implementations.NotificationModule.Jobs
{
    public sealed class PrescriptionExpiryWarningJob(
       IUnitOfWork _unitOfWork,
       INotificationService _notificationService,
       ILogger<PrescriptionExpiryWarningJob> _logger)
    {
        public async Task ExecuteAsync()
        {
            var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

            var prescRepo = _unitOfWork.GetRepository<Prescription, int>();
            var prescriptions = (await prescRepo.GetAllAsync(asNoTracking: true))
                .Where(p => p.Status == PrescriptionStatus.Active && p.ExpiresAt == targetDate)
                .ToList();

            if (!prescriptions.Any())
            {
                _logger.LogInformation("[PrescriptionExpiryWarningJob] No prescriptions expiring on {Date}.", targetDate);
                return;
            }

            var notifRepo = _unitOfWork.GetRepository<Notification, Guid>();
            int sent = 0;

            foreach (var prescription in prescriptions)
            {
                // Deduplication: skip if already warned in last 7 days for same prescription
                var alreadySent = await notifRepo.CountAsync(
                    new NotificationLogForDeduplicationSpec(
                        prescription.Id.ToString(),
                        NotificationType.PrescriptionExpiryWarning,
                        lookBackDays: 7)) > 0;

                if (alreadySent)
                {
                    _logger.LogDebug("[PrescriptionExpiryWarningJob] Already warned for prescription {Id}.", prescription.Id);
                    continue;
                }

                try
                {
                    await _notificationService.SendPrescriptionExpiryWarningAsync(
                        prescription.PatientId, prescription.Id, prescription.ExpiresAt);
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[PrescriptionExpiryWarningJob] Failed for prescription {Id}.", prescription.Id);
                }
            }

            _logger.LogInformation("[PrescriptionExpiryWarningJob] Sent {Count} expiry warnings.", sent);
        }
    }
}
