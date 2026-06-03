using HMS.DAL.Models.MedicalRecordModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.MedicalRecordConfigs
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("Prescriptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MedicationName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Dosage)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Frequency)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Instructions)
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.PrescribedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.IsControlledSubstance)
                .HasDefaultValue(false);


            builder.HasIndex(x => x.PatientId)
                .HasDatabaseName("IX_Prescriptions_PatientId");

            builder.HasIndex(x => x.MedicalRecordId)
                .HasDatabaseName("IX_Prescriptions_MedicalRecordId");

            #region RelationShips

            builder.HasOne(x => x.MedicalRecord)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(x => x.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);



            #endregion
        }
    }
}
