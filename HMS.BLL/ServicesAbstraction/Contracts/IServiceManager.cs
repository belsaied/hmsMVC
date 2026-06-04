using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.ServicesAbstraction.Contracts.WardBedService;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IServiceManager
    {
        #region Patient
        public IPatientService PatientService { get; }
        public IAllergyService AllergyService { get; }
        public IMedicalHistoryService MedicalHistoryService { get; }
        public IEmergencyContactService EmergencyContactService { get; }
        #endregion

        #region Doctor
        public IDoctorService DoctorService { get; }
        public IDepartmentService DepartmentService { get; }

        #endregion

        #region Appointment
        public IAppointmentService AppointmentService { get; }
        #endregion

        #region Medical Records
        public IMedicalRecordService MedicalRecordService { get; }
        public IVitalSignService VitalSignService { get; }
        public IPrescriptionService PrescriptionService { get; }
        public ILabOrderService LabOrderService { get; }
        #endregion

        #region WardBed
        public IWardService WardService { get; }
        public IBedService BedService { get; }
        public IAdmissionService AdmissionService { get; }

        #endregion

        #region Identity
        public IAuthService AuthService { get; }
        public IAuditService AuditService { get; }
        public IEmailService EmailService { get; }
        #endregion

        #region Billing
        public IInvoiceService InvoiceService { get; }
        public IPaymentService PaymentService { get; }
        public IInsuranceService InsuranceService { get; }
        public IReportingService ReportingService { get; }
        #endregion

        #region Notification
        public INotificationService NotificationService { get; }
        public INotificationPreferenceService NotificationPreferenceService { get; }
        public INotificationLogService NotificationLogService { get; }
        public IAdminNotificationLogService AdminNotificationLogService { get; }
        #endregion
        

        #region Cash
        ICacheService CacheService { get; }
        #endregion
    }
}
