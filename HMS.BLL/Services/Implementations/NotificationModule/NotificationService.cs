using HMS.DAL.Contracts;
using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using Microsoft.Extensions.Logging;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.NotificationModule;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationTemplateSpecifications;
using HMS.BLL.Shared.Dtos.NotificationDtos.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Implementations.NotificationModule
{
    public sealed class NotificationService(
       IUnitOfWork _unitOfWork,
       IEmailSender _emailSender,
       ISmsSender _smsSender,
       INotificationPushSender _pushSender,
       ILogger<NotificationService> _logger) : INotificationService
    {
        // ── Public Send Methods ─────────────────────────────────────────────────

        public async Task SendAppointmentConfirmedAsync(AppointmentNotificationEvent evt)
        {
            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = evt.PatientName,
                ["DoctorName"] = evt.DoctorName,
                ["AppointmentDate"] = evt.AppointmentDate.ToString("dd MMM yyyy"),
                ["StartTime"] = evt.StartTime.ToString("HH:mm"),
                ["ConfirmationNumber"] = evt.ConfirmationNumber
            };

            var recipientUserId = evt.PatientId.ToString();

            await SendOnChannelAsync(
                recipientUserId, RecipientType.Patient,
                evt.PatientEmail, evt.PatientPhone,
                NotificationType.AppointmentConfirmed,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.AppointmentId.ToString(),
                relatedEntityType: "Appointment");

            await SendOnChannelAsync(
                recipientUserId, RecipientType.Patient,
                evt.PatientEmail, evt.PatientPhone,
                NotificationType.AppointmentConfirmed,
                NotificationChannel.SMS,
                placeholders,
                relatedEntityId: evt.AppointmentId.ToString(),
                relatedEntityType: "Appointment");
        }

        public async Task SendAppointmentCancelledAsync(AppointmentNotificationEvent evt)
        {
            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = evt.PatientName,
                ["DoctorName"] = evt.DoctorName,
                ["AppointmentDate"] = evt.AppointmentDate.ToString("dd MMM yyyy"),
                ["StartTime"] = evt.StartTime.ToString("HH:mm")
            };

            // Notify patient
            await SendOnChannelAsync(
                evt.PatientId.ToString(), RecipientType.Patient,
                evt.PatientEmail, evt.PatientPhone,
                NotificationType.AppointmentCancelled,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.AppointmentId.ToString(),
                relatedEntityType: "Appointment");

            // Notify doctor
            await SendOnChannelAsync(
                evt.DoctorId.ToString(), RecipientType.Doctor,
                evt.DoctorEmail, null,
                NotificationType.AppointmentCancelled,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.AppointmentId.ToString(),
                relatedEntityType: "Appointment");
        }

        public async Task SendAppointmentReminderAsync(int patientId, int appointmentId)
        {
            // Fetch patient contact info from DB
            var patientRepo = _unitOfWork.GetRepository<HMS.DAL.Models.PatientModule.Patient, int>();
            var patient = await patientRepo.GetByIdAsync(patientId);
            if (patient is null)
            {
                _logger.LogWarning("[AppointmentReminder] Patient {PatientId} not found.", patientId);
                return;
            }

            var aptRepo = _unitOfWork.GetRepository<HMS.DAL.Models.AppointmentModule.Appointment, int>();
            var appointment = await aptRepo.GetByIdAsync(
                new Services.Specifications.AppointmentModule.AppointmentWithDetailsSpecification(appointmentId));
            if (appointment is null) return;

            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = $"{patient.FirstName} {patient.LastName}",
                ["DoctorName"] = $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
                ["AppointmentDate"] = appointment.AppointmentDate.ToString("dd MMM yyyy"),
                ["StartTime"] = appointment.StartTime.ToString("HH:mm"),
                ["ConfirmationNumber"] = appointment.ConfirmationNumber
            };

            var recipientUserId = patientId.ToString();

            await SendOnChannelAsync(
                recipientUserId, RecipientType.Patient,
                patient.Email, patient.Phone,
                NotificationType.AppointmentReminder,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: appointmentId.ToString(),
                relatedEntityType: "Appointment");

            await SendOnChannelAsync(
                recipientUserId, RecipientType.Patient,
                patient.Email, patient.Phone,
                NotificationType.AppointmentReminder,
                NotificationChannel.SMS,
                placeholders,
                relatedEntityId: appointmentId.ToString(),
                relatedEntityType: "Appointment");
        }

        public async Task SendAbnormalLabResultAsync(AbnormalLabResultEvent evt)
        {
            var placeholders = new Dictionary<string, string?>
            {
                ["DoctorName"] = evt.DoctorName,
                ["PatientName"] = evt.PatientName,
                ["TestName"] = evt.TestName,
                ["ResultValue"] = evt.ResultValue,
                ["NormalRange"] = evt.NormalRange
            };

            var doctorUserId = evt.OrderingDoctorId.ToString();

            await SendOnChannelAsync(
                doctorUserId, RecipientType.Doctor,
                evt.DoctorEmail, null,
                NotificationType.AbnormalLabResult,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.LabOrderId.ToString(),
                relatedEntityType: "LabOrder");

            await SendOnChannelAsync(
                doctorUserId, RecipientType.Doctor,
                evt.DoctorEmail, null,
                NotificationType.AbnormalLabResult,
                NotificationChannel.Push,
                placeholders,
                relatedEntityId: evt.LabOrderId.ToString(),
                relatedEntityType: "LabOrder");
        }

        public async Task SendPrescriptionExpiryWarningAsync(int patientId, int prescriptionId, DateOnly expiryDate)
        {
            var patientRepo = _unitOfWork.GetRepository<HMS.DAL.Models.PatientModule.Patient, int>();
            var patient = await patientRepo.GetByIdAsync(patientId);
            if (patient is null) return;

            var prescRepo = _unitOfWork.GetRepository<HMS.DAL.Models.MedicalRecordModule.Prescription, int>();
            var prescription = await prescRepo.GetByIdAsync(prescriptionId);
            if (prescription is null) return;

            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = $"{patient.FirstName} {patient.LastName}",
                ["MedicationName"] = prescription.MedicationName,
                ["ExpiryDate"] = expiryDate.ToString("dd MMM yyyy")
            };

            await SendOnChannelAsync(
                patientId.ToString(), RecipientType.Patient,
                patient.Email, patient.Phone,
                NotificationType.PrescriptionExpiryWarning,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: prescriptionId.ToString(),
                relatedEntityType: "Prescription");
        }

        public async Task SendInvoiceIssuedAsync(InvoiceNotificationEvent evt)
        {
            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = evt.PatientName,
                ["InvoiceNumber"] = evt.InvoiceNumber,
                ["TotalAmount"] = evt.TotalAmount.ToString("N2"),
                ["DueDate"] = evt.DueDate?.ToString("dd MMM yyyy") ?? "N/A"
            };

            await SendOnChannelAsync(
                evt.PatientId.ToString(), RecipientType.Patient,
                evt.PatientEmail, null,
                NotificationType.InvoiceIssued,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.InvoiceId.ToString(),
                relatedEntityType: "Invoice",
                pdfBytes: evt.PdfBytes,
                pdfFileName: $"invoice-{evt.InvoiceNumber}.pdf");
        }

        public async Task SendPaymentReceivedAsync(PaymentNotificationEvent evt)
        {
            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = evt.PatientName,
                ["InvoiceNumber"] = evt.InvoiceNumber,
                ["AmountPaid"] = evt.AmountPaid.ToString("N2"),
                ["PaidAt"] = evt.PaidAt.ToString("dd MMM yyyy HH:mm")
            };

            await SendOnChannelAsync(
                evt.PatientId.ToString(), RecipientType.Patient,
                evt.PatientEmail, null,
                NotificationType.PaymentReceived,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.InvoiceId.ToString(),
                relatedEntityType: "Invoice");
        }

        public async Task SendInvoiceOverdueAsync(InvoiceNotificationEvent evt)
        {
            var placeholders = new Dictionary<string, string?>
            {
                ["PatientName"] = evt.PatientName,
                ["InvoiceNumber"] = evt.InvoiceNumber,
                ["TotalAmount"] = evt.TotalAmount.ToString("N2"),
                ["DueDate"] = evt.DueDate?.ToString("dd MMM yyyy") ?? "N/A",
                ["OutstandingBalance"] = evt.OutstandingBalance.ToString("N2")
            };

            await SendOnChannelAsync(
                evt.PatientId.ToString(), RecipientType.Patient,
                evt.PatientEmail, null,
                NotificationType.InvoiceOverdue,
                NotificationChannel.Email,
                placeholders,
                relatedEntityId: evt.InvoiceId.ToString(),
                relatedEntityType: "Invoice");
        }

        public async Task SendCustomAdminMessageAsync(int recipientId, RecipientType recipientType, string subject, string body, NotificationChannel channel
            , string? recipientEmail = null,     
              string? recipientPhone = null)
        {
            // For admin custom messages: bypass template system, bypass preferences
            var notification = new Notification
            {
                RecipientUserId = recipientId.ToString(),
                RecipientType = recipientType,
                Channel = channel,
                NotificationType = NotificationType.CustomAdminMessage,
                Subject = subject,
                Body = body,
                DeliveryStatus = DeliveryStatus.Pending
            };

            await _unitOfWork.GetRepository<Notification, Guid>().AddAsync(notification);

            try
            {
                switch (channel)
                {
                    case NotificationChannel.Email:
                        if (string.IsNullOrWhiteSpace(recipientEmail))
                            throw new NotificationChannelUnavailableException("Email");
                        await _emailSender.SendAsync(recipientEmail, string.Empty, subject, body);
                        break;

                    case NotificationChannel.SMS:
                        if (string.IsNullOrWhiteSpace(recipientPhone))
                            throw new NotificationChannelUnavailableException("SMS");
                        var sid = await _smsSender.SendAsync(recipientPhone, body);
                        break;
                    case NotificationChannel.Push:
                        await _pushSender.SendAsync(recipientId.ToString(), new { subject, body });
                        notification.DeliveryStatus = DeliveryStatus.Sent;
                        notification.SentAt = DateTimeOffset.UtcNow;
                        break;
                }
            }
            catch (Exception ex)
            {
                notification.DeliveryStatus = DeliveryStatus.Failed;
                notification.FailureReason = ex.Message;
                _logger.LogError(ex, "[CustomAdminMessage] Failed to send to recipient {Id}", recipientId);
            }

            _unitOfWork.GetRepository<Notification, Guid>().Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Core Pipeline ───────────────────────────────────────────────────────

        private async Task SendOnChannelAsync(
            string recipientUserId,
            RecipientType recipientType,
            string? recipientEmail,
            string? recipientPhone,
            NotificationType notificationType,
            NotificationChannel channel,
            Dictionary<string, string?> placeholders,
            string? relatedEntityId = null,
            string? relatedEntityType = null,
            byte[]? pdfBytes = null,
            string? pdfFileName = null)
        {
            // 1. Check preference (opt-out model)
            if (!await IsEnabledAsync(recipientUserId, notificationType, channel))
            {
                await LogAsync(new Notification
                {
                    RecipientUserId = recipientUserId,
                    RecipientType = recipientType,
                    RecipientEmail = recipientEmail,
                    RecipientPhone = recipientPhone,
                    Channel = channel,
                    NotificationType = notificationType,
                    Body = "Skipped: user preference disabled.",
                    DeliveryStatus = DeliveryStatus.Skipped,
                    FailureReason = "User has disabled this notification type/channel.",
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType
                });
                return;
            }

            // 2. SMS requires a phone number
            if (channel == NotificationChannel.SMS && string.IsNullOrEmpty(recipientPhone))
            {
                await LogAsync(new Notification
                {
                    RecipientUserId = recipientUserId,
                    RecipientType = recipientType,
                    Channel = channel,
                    NotificationType = notificationType,
                    Body = "Skipped: no phone number.",
                    DeliveryStatus = DeliveryStatus.Skipped,
                    FailureReason = "Recipient has no phone number on file.",
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType
                });
                return;
            }

            // 3. Resolve template
            var templateRepo = _unitOfWork.GetRepository<NotificationTemplate, int>();
            var template = await templateRepo.GetByIdAsync(new ActiveTemplateSpec(notificationType, channel));
            if (template is null)
            {
                await LogAsync(new Notification
                {
                    RecipientUserId = recipientUserId,
                    RecipientType = recipientType,
                    RecipientEmail = recipientEmail,
                    RecipientPhone = recipientPhone,
                    Channel = channel,
                    NotificationType = notificationType,
                    Body = "Skipped: no active template found.",
                    DeliveryStatus = DeliveryStatus.Skipped,
                    FailureReason = $"No active template for {notificationType} / {channel}.",
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType
                });
                return;
            }

            // 4. Render template
            var renderedSubject = template.SubjectTemplate != null
                ? TemplateRenderer.Render(template.SubjectTemplate, placeholders)
                : null;
            var renderedBody = TemplateRenderer.Render(template.BodyTemplate, placeholders);

            // 5. Create notification log entry
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                RecipientType = recipientType,
                RecipientEmail = recipientEmail,
                RecipientPhone = recipientPhone,
                Channel = channel,
                NotificationType = notificationType,
                Subject = renderedSubject,
                Body = renderedBody,
                DeliveryStatus = DeliveryStatus.Pending,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };

            await _unitOfWork.GetRepository<Notification, Guid>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // 6. Dispatch
            try
            {
                switch (channel)
                {
                    case NotificationChannel.Email:
                        await _emailSender.SendAsync(
                            recipientEmail!, string.Empty,
                            renderedSubject ?? string.Empty,
                            renderedBody,
                            pdfBytes, pdfFileName);
                        break;

                    case NotificationChannel.SMS:
                        var sid = await _smsSender.SendAsync(recipientPhone!, renderedBody);
                        notification.ExternalMessageId = sid;
                        break;

                    case NotificationChannel.Push:
                        bool connected = await _pushSender.IsUserConnectedAsync(recipientUserId);
                        if (connected)
                        {
                            await _pushSender.SendAsync(recipientUserId, new
                            {
                                notificationId = notification.Id,
                                type = notificationType.ToString(),
                                subject = renderedSubject,
                                body = renderedBody,
                                relatedEntityId = relatedEntityId,
                                relatedEntityType = relatedEntityType,
                                createdAt = notification.CreatedAt
                            });
                        }
                        // If not connected: stays Pending/IsRead=false — frontend fetches on reconnect
                        break;
                }

                notification.DeliveryStatus = DeliveryStatus.Sent;
                notification.SentAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                notification.DeliveryStatus = DeliveryStatus.Failed;
                notification.FailureReason = ex.Message[..Math.Min(ex.Message.Length, 500)];
                _logger.LogError(ex,
                    "[NotificationService] Failed to send {Type} via {Channel} to user {UserId}",
                    notificationType, channel, recipientUserId);
            }

            _unitOfWork.GetRepository<Notification, Guid>().Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private async Task<bool> IsEnabledAsync(string userId, NotificationType type, NotificationChannel channel)
        {
            var prefRepo = _unitOfWork.GetRepository<NotificationPreference, int>();
            var pref = await prefRepo.GetByIdAsync(new PreferenceByUserTypeChannelSpec(userId, type, channel));
            // Opt-out model: if no preference row → default is enabled
            return pref?.IsEnabled ?? true;
        }

        private async Task LogAsync(Notification notification)
        {
            await _unitOfWork.GetRepository<Notification, Guid>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
