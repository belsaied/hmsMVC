using HMS.DAL.Models.MedicalRecordModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.MedicalRecordConfigs
{
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("MedicalRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ChiefComplaint)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Diagnosis)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.IcdCode)
                .HasMaxLength(500);

            builder.Property(x => x.ClinicalNotes)
                .HasMaxLength(4000);

            builder.Property(x => x.TreatmentPlan)
                .HasMaxLength(2000);

            builder.Property(x => x.VisitDate)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.RecordedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("datetime2");

            builder.Property(x => x.FollowUpDate)
                .HasColumnType("date");

            builder.Property(x => x.IsConfidential)
                .HasDefaultValue(false)
                .IsRequired();

            //Indexes
            builder.HasIndex(x => x.PatientId)
                .HasDatabaseName("IX_MedicalRecords_PatientId");

            builder.HasIndex(x => x.DoctorId)
                .HasDatabaseName("IX_MedicalRecords_DoctorId");

            builder.HasIndex(x => x.AppointmentId)
                .HasDatabaseName("IX_MedicalRecords_AppointmentId");

            #region RelationShips
            //Many=>MedicalRecord to one =>Patient 
            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            //Many=>MedicalRecord to one =>Doctor 
            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            //Many=>MedicalRecord to one =>Appointment 
            builder.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            #endregion
        }
    }
}
