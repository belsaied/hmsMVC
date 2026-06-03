using HMS.DAL.Models.MedicalRecordModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.MedicalRecordConfigs
{
    public class VitalSignConfiguration : IEntityTypeConfiguration<VitalSign>
    {
        public void Configure(EntityTypeBuilder<VitalSign> builder)
        {
            builder.ToTable("VitalSigns");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Temperature)
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.OxygenSaturation)
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.Weight)
                .HasColumnType("decimal(6,2)");

            builder.Property(x => x.Height)
                .HasColumnType("decimal(6,2)");

            builder.Property(x => x.RecordedBy)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RecordedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.MedicalRecordId);


            #region RelationShips
            builder.HasOne(x => x.MedicalRecord)
                .WithMany(m => m.VitalSigns)
                .HasForeignKey(x => x.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);  // لو الريكورد اتحذف، الـ vitals تتحذف معاه

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.NoAction);   // NoAction عشان مفيش cascade conflict مع الـ MedicalRecord → Patient


            #endregion
        }
    }
}
