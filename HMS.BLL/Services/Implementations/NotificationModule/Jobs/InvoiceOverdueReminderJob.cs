using HMS.BLL.Services.Specifications.BillingModule;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Shared.Dtos.NotificationDtos.Events;
using HMS.DAL.Contracts;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using HMS.DAL.Models.PatientModule;
using Microsoft.Extensions.Logging;

namespace HMS.BLL.Services.Implementations.NotificationModule.Jobs
{
    public sealed class InvoiceOverdueReminderJob(
        IUnitOfWork _unitOfWork,
        INotificationService _notificationService,
        ILogger<InvoiceOverdueReminderJob> _logger)
    {
        public async Task ExecuteAsync()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var sevenDaysAgo = now.AddDays(-7);
            var yesterday = now.AddDays(-1);

            var invoiceRepo = _unitOfWork.GetRepository<Invoice, Guid>();
            // DueDate between 1 and 7 days ago
            var overdueInvoices = (await invoiceRepo.GetAllAsync(
                new InvoicesByStatusSpecification(new[] { InvoiceStatus.Overdue })))
                .Where(i => i.DueDate.HasValue
                         && i.DueDate.Value >= sevenDaysAgo
                         && i.DueDate.Value <= yesterday)
                .ToList();

            if (!overdueInvoices.Any())
            {
                _logger.LogInformation("[InvoiceOverdueReminderJob] No overdue invoices to remind.");
                return;
            }

            var notifRepo = _unitOfWork.GetRepository<Notification, Guid>();
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            int sent = 0;

            foreach (var invoice in overdueInvoices)
            {
                // Deduplication: skip if InvoiceOverdue notification sent in last 7 days
                var alreadySent = await notifRepo.CountAsync(
                    new NotificationLogForDeduplicationSpec(
                        invoice.Id.ToString(),
                        NotificationType.InvoiceOverdue,
                        lookBackDays: 7)) > 0;

                if (alreadySent)
                {
                    _logger.LogDebug("[InvoiceOverdueReminderJob] Already notified for invoice {Id}.", invoice.Id);
                    continue;
                }

                var patient = await patientRepo.GetByIdAsync(invoice.PatientId);
                if (patient is null) continue;

                try
                {
                    var evt = new InvoiceNotificationEvent
                    {
                        InvoiceId = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        PatientId = invoice.PatientId,
                        PatientEmail = patient.Email,
                        PatientName = $"{patient.FirstName} {patient.LastName}",
                        TotalAmount = invoice.TotalAmount,
                        OutstandingBalance = invoice.OutstandingBalance,
                        DueDate = invoice.DueDate
                    };

                    await _notificationService.SendInvoiceOverdueAsync(evt);
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[InvoiceOverdueReminderJob] Failed for invoice {Id}.", invoice.Id);
                }
            }

            _logger.LogInformation("[InvoiceOverdueReminderJob] Sent {Count} overdue reminders.", sent);
        }
    }
}
