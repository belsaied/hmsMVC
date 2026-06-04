using HMS.BLL.Services.Implementations.BillingModule;
using HMS.BLL.Services.Implementations.NotificationModule.Jobs;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Shared.Common.NotificationSettings;
using HMS.PL.Senders;

namespace HMS.PL.Extensions
{
    public static class NotificationServiceExtensions
    {
        public static IServiceCollection AddNotificationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Settings ──────────────────────────────────────────────────────
            services.Configure<NotificationEmailSettings>(
                configuration.GetSection("NotificationEmailSettings"));

            services.Configure<TwilioSettings>(
                configuration.GetSection("TwilioSettings"));

            // ── Email Sender (provider-switched) ──────────────────────────────
            var provider = configuration["NotificationEmailSettings:Provider"] ?? "Smtp";
            if (provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
                services.AddScoped<IEmailSender, SendGridEmailSender>();
            else
                services.AddScoped<IEmailSender, SmtpEmailSender>();

            // ── SMS Sender ────────────────────────────────────────────────────
            services.AddScoped<ISmsSender, TwilioSmsSender>();

            // ── Push Sender (SignalR) ─────────────────────────────────────────
            services.AddScoped<INotificationPushSender, NotificationPushSender>();


            // ── Hangfire Jobs ─────────────────────────────────────────────────
            services.AddTransient<AppointmentReminderJob>();
            services.AddTransient<PrescriptionExpiryWarningJob>();
            services.AddTransient<InvoiceOverdueReminderJob>();
            services.AddTransient<MarkOverdueInvoicesJob>();
            services.AddTransient<InvoiceExpiryNotificationJob>();
            return services;
        }
    }
}