using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.ServicesAbstraction.Contracts.WardBedService;

namespace HMS.BLL.Services.Implementations
{
    public class ServiceManagerWithFactoryDelegate(
        //Patient Module
        Func<IPatientService> _patientService  ,
        Func<IAllergyService> _allergyService  ,
        Func<IEmergencyContactService> _emergencyContactService,
        Func<IMedicalHistoryService> _medicalHistoryService,
        //Doctor Module
        Func<IDoctorService> _doctorService,
        Func<IDepartmentService> _departmentService,
        Func<IAppointmentService> _appointmentService,
                // Medical Records Module
        Func<IMedicalRecordService> _medicalRecordService,
        Func<IVitalSignService> _vitalSignService,
        Func<IPrescriptionService> _prescriptionService,
        Func<ILabOrderService> _labOrderService,
        //WardBed Module
        Func<IWardService>_wardService,
        Func<IBedService>_bedService,
        Func<IAdmissionService> _admissionService,
        Func<IAuthService> _authService,
        Func<IAuditService> _auditService,
        Func<IEmailService> _emailService,
        // Billing Module
        Func<IInvoiceService> _invoiceService,
        Func<IPaymentService> _paymentService,
        Func<IInsuranceService> _insuranceService,
        Func<IReportingService> _reportingService,
       //Notification Module
        Func<INotificationService> _notificationService,
        Func<INotificationPreferenceService> _notificationPreferenceService,
        Func<INotificationLogService> _notificationLogService,
        Func<IAdminNotificationLogService> _adminNotificationLogService,
         //Cash
        Func<ICacheService> _cacheService
        ) : IServiceManager
    {
        //Patient Module
        public IPatientService PatientService => _patientService.Invoke();

        public IAllergyService AllergyService => _allergyService.Invoke();

        public IEmergencyContactService EmergencyContactService => _emergencyContactService.Invoke();

        public IMedicalHistoryService MedicalHistoryService => _medicalHistoryService.Invoke();
        
        //Doctor Module
        public IDoctorService DoctorService => _doctorService.Invoke();

        public IDepartmentService DepartmentService => _departmentService.Invoke();

        public IAppointmentService AppointmentService => _appointmentService.Invoke();

        // Medical Records Module
        public IMedicalRecordService MedicalRecordService => _medicalRecordService.Invoke();
        public IVitalSignService VitalSignService => _vitalSignService.Invoke();
        public IPrescriptionService PrescriptionService => _prescriptionService.Invoke();
        public ILabOrderService LabOrderService => _labOrderService.Invoke();

        public IWardService WardService => _wardService.Invoke();

        public IBedService BedService => _bedService.Invoke();

        public IAdmissionService AdmissionService => _admissionService.Invoke();

        // Identiy
        public IAuthService AuthService => _authService.Invoke();
        public IAuditService AuditService => _auditService.Invoke();
        public IEmailService EmailService => _emailService.Invoke();

        // Billing Module
        public IInvoiceService InvoiceService => _invoiceService.Invoke();
        public IPaymentService PaymentService => _paymentService.Invoke();
        public IInsuranceService InsuranceService => _insuranceService.Invoke();
        public IReportingService ReportingService => _reportingService.Invoke();

        // Notification Module
        public INotificationService NotificationService => _notificationService.Invoke();
        public INotificationPreferenceService NotificationPreferenceService => _notificationPreferenceService.Invoke();
        public INotificationLogService NotificationLogService => _notificationLogService.Invoke();
        public IAdminNotificationLogService AdminNotificationLogService => _adminNotificationLogService.Invoke();

        //Cashing
        public ICacheService CacheService => _cacheService.Invoke();
    }
}
