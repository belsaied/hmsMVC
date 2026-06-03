using HMS.DAL.Models.DoctorModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.DoctorModuleConfigs
{
    public class DoctorQualificationConfiguration : IEntityTypeConfiguration<DoctorQualification>
    {
        public void Configure(EntityTypeBuilder<DoctorQualification> builder)
        {
            builder.ToTable("DoctorQualifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Degree)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Institution)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Country)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.YearAwarded)
                .IsRequired();

            builder.HasIndex(x => x.DoctorId);

            #region Relationships
            builder.HasOne(x => x.Doctor)
                .WithMany(d => d.Qualifications)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade: لو الدكتور اتحذف، كل مؤهلاته تتحذف معاه

            #endregion

        }
    }
}
