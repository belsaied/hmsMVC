using HMS.DAL.Models.MedicalRecordModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.MedicalRecordConfigs
{
    public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
    {
        public void Configure(EntityTypeBuilder<LabOrder> builder)
        {
            builder.ToTable("LabOrders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TestName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.TestCode)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.Priority)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.OrderedAt)
                .HasColumnType("datetime2")
                .IsRequired();


            builder.HasIndex(x => x.PatientId)
                .HasDatabaseName("IX_LabOrders_PatientId");

            builder.HasIndex(x => x.MedicalRecordId)
                .HasDatabaseName("IX_LabOrders_MedicalRecordId");

            #region RelationShips
            builder.HasOne(x => x.MedicalRecord)
                .WithMany(m => m.LabOrders)
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
