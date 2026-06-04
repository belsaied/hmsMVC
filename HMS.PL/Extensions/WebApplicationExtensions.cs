using HMS.DAL.Contracts;
using HMS.DAL.Data.Identity;
using Hangfire;
using HMS.PL.MiddleWares;
using HMS.BLL.Services.Implementations.BillingModule;
using HMS.BLL.Services.Implementations.NotificationModule.Jobs;

namespace HMS.PL.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var objOfDataSeeding = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
            await objOfDataSeeding.SeedDataAsync();
            var identitySeeding = scope.ServiceProvider.GetRequiredService<IdentityDataSeeding>();
            await identitySeeding.SeedAsync();
            return app;
        }

        public static WebApplication UseExceptionHandlingMiddlewares(this WebApplication app)
        {
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            return app;
        }

        public static WebApplication RegisterBillingRecurringJobs(this WebApplication app)
        {
            RecurringJob.AddOrUpdate<MarkOverdueInvoicesJob>(
                recurringJobId: "billing-mark-overdue",
                methodCall: j => j.ExecuteAsync(),
                cronExpression: "5 0 * * *",
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            RecurringJob.AddOrUpdate<InvoiceExpiryNotificationJob>(
                recurringJobId: "billing-expiry-reminders",
                methodCall: j => j.ExecuteAsync(),
                cronExpression: "0 8 * * *",
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            RecurringJob.AddOrUpdate<AppointmentReminderJob>(
                recurringJobId: "notification-appointment-reminder",
                methodCall: j => j.ExecuteAsync(),
                cronExpression: "0 8 * * *",
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            RecurringJob.AddOrUpdate<PrescriptionExpiryWarningJob>(
                recurringJobId: "notification-prescription-expiry",
                methodCall: j => j.ExecuteAsync(),
                cronExpression: "0 9 * * *",
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            RecurringJob.AddOrUpdate<InvoiceOverdueReminderJob>(
                recurringJobId: "notification-invoice-overdue",
                methodCall: j => j.ExecuteAsync(),
                cronExpression: "0 10 * * *",
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            return app;
        }
    }
}