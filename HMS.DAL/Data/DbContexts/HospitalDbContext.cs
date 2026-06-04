using HMS.DAL.Models.AppointmentModule;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.DAL.Models.NotificationModule;
using HMS.DAL.Models.PatientModule;
using HMS.DAL.Models.WardBedModule;
using Microsoft.EntityFrameworkCore;

namespace HMS.DAL.Data.DbContexts
{
    public class HospitalDbContext:DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options):base(options)
        {
            
        }
        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HospitalDbContext).Assembly);

        }
        #region Patient Module
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientMedicalHistory> PatientMedicalHistories { get; set; }
        public DbSet<PatientAllergy> PatientAllergies { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        #endregion

        #region Doctor Module
        public DbSet<Department> Departments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorQualification> DoctorQualifications { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

        #endregion

        #region AppointmentModule
        public DbSet<Appointment> Appointments { get; set; }
        #endregion

        #region Medical Record Medule
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<VitalSign> VitalSigns { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<LabResult> LabResults { get; set; }
        #endregion

        #region WardBed Module
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<BedTransfer> BedTransfers { get; set; }
        #endregion

        #region BillingModule
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; set; }
        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
        #endregion
       
        #region Notification Module
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        #endregion

    }
}
