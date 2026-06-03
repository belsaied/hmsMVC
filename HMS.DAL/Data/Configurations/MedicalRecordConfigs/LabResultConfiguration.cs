using HMS.DAL.Models.MedicalRecordModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.MedicalRecordConfigs
{
    public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
    {
        public void Configure(EntityTypeBuilder<LabResult> builder)
        {
            builder.ToTable("LabResults");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ResultText)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(x => x.NormalRange)
                .HasMaxLength(200);

            builder.Property(x => x.PerformedBy)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PerformedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.ReportedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.IsAbnormal)
                .HasDefaultValue(false);


            builder.HasIndex(x => x.LabOrderId)
                .IsUnique()
                .HasDatabaseName("IX_LabResults_LabOrderId");


            #region RelationShips

            // One-to-One: LabResult ←→ LabOrder
            builder.HasOne(x => x.LabOrder)
                .WithOne(o => o.Result)
                .HasForeignKey<LabResult>(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Cascade); // لو الـ order اتحذف، الـ result يتحذف معاه

            #endregion
        }
    }
}
