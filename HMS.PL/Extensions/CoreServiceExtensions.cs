// HMS.PL/Extensions/CoreServiceExtensions.cs
using HMS.BLL.Services.Implementations;
using HMS.BLL.Services.Implementations.AppointmentModule;
using HMS.BLL.Services.Implementations.BillingModule;
using HMS.BLL.Services.Implementations.DoctorModule;
using HMS.BLL.Services.Implementations.MedicalRecordModule;
using HMS.BLL.Services.Implementations.NotificationModule;
using HMS.BLL.Services.Implementations.PatientModule;
using HMS.BLL.Services.Implementations.UserManagementModule;
using HMS.BLL.Services.Implementations.WardBedModule;
using HMS.BLL.Services.MappingProfiles.PatientModule;
using HMS.BLL.Services.MappingProfiles.DoctorModule;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.ServicesAbstraction.Contracts.WardBedService;
using HMS.BLL.Shared.Common;
using HMS.PL.Authorization;
using HMS.PL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Services.Implementations.BillingModule;
using Services.Implementations.MedicalRecordModule;

namespace HMS.PL.Extensions
{
    public static class CoreServiceExtensions
    {
        public static IServiceCollection AddCoreServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Email (auth emails) ────────────────────────────────────────────
            services.Configure<EmailOptions>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // ── BLL Services ───────────────────────────────────────────────────
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IAllergyService, AllergyService>();
            services.AddScoped<IEmergencyContactService, EmergencyContactService>();
            services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<IVitalSignService, VitalSignService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();
            services.AddScoped<ILabOrderService, LabOrderService>();
            services.AddScoped<IWardService, WardService>();
            services.AddScoped<IBedService, BedService>();
            services.AddScoped<IAdmissionService, AdmissionService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IInsuranceService, InsuranceService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
            services.AddScoped<IPrescriptionPdfGenerator, PrescriptionPdfGenerator>();
            services.AddScoped<IAdminNotificationLogService, AdminNotificationLogService>();

            // ── SignalR notifiers ──────────────────────────────────────────────
            services.AddScoped<IAppointmentNotifier, AppointmentNotifier>();
            services.AddScoped<IBedNotifier, BedNotifier>();
            services.AddScoped<IInvoiceNotifier, InvoiceNotifier>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
            services.AddScoped<INotificationLogService, NotificationLogService>();

            // ── AutoMapper (v16 API) ───────────────────────────────────────────
            services.AddAutoMapper(cfg =>
                cfg.AddMaps(typeof(HMS.BLL.Services.ServiceAssemblyReference).Assembly));
            services.AddTransient(typeof(PatientPictureUrlResolver<>));
            services.AddTransient(typeof(DoctorPictureUrlResolver<>));
            // ── Service Manager (factory delegate pattern) ────────────────────
            services.AddScoped<IServiceManager, ServiceManagerWithFactoryDelegate>(sp =>
                new ServiceManagerWithFactoryDelegate(
                    () => sp.GetRequiredService<IPatientService>(),
                    () => sp.GetRequiredService<IAllergyService>(),
                    () => sp.GetRequiredService<IEmergencyContactService>(),
                    () => sp.GetRequiredService<IMedicalHistoryService>(),
                    () => sp.GetRequiredService<IDoctorService>(),
                    () => sp.GetRequiredService<IDepartmentService>(),
                    () => sp.GetRequiredService<IAppointmentService>(),
                    () => sp.GetRequiredService<IMedicalRecordService>(),
                    () => sp.GetRequiredService<IVitalSignService>(),
                    () => sp.GetRequiredService<IPrescriptionService>(),
                    () => sp.GetRequiredService<ILabOrderService>(),
                    () => sp.GetRequiredService<IWardService>(),
                    () => sp.GetRequiredService<IBedService>(),
                    () => sp.GetRequiredService<IAdmissionService>(),
                    () => sp.GetRequiredService<IAuthService>(),
                    () => sp.GetRequiredService<IAuditService>(),
                    () => sp.GetRequiredService<IEmailService>(),
                    () => sp.GetRequiredService<IInvoiceService>(),
                    () => sp.GetRequiredService<IPaymentService>(),
                    () => sp.GetRequiredService<IInsuranceService>(),
                    () => sp.GetRequiredService<IReportingService>(),
                    () => sp.GetRequiredService<INotificationService>(),
                    () => sp.GetRequiredService<INotificationPreferenceService>(),
                    () => sp.GetRequiredService<INotificationLogService>(),
                    () => sp.GetRequiredService<IAdminNotificationLogService>(),
                    () => sp.GetRequiredService<ICacheService>()
                ));

            // ── Authorization ─────────────────────────────────────────────────
            services.AddScoped<IAuthorizationHandler, PatientOwnershipHandler>();
            services.AddHttpContextAccessor();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PatientOwnership",
                    policy => policy.Requirements.Add(new PatientOwnershipRequirement()));
            });

            return services;
        }
    }
}