using HMS.DAL.Models.AppointmentModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.AppointmentConfigs
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Type)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.Property(x => x.ReasonForVisit)
                   .HasMaxLength(500).IsRequired();

            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.CancellationReason).HasMaxLength(500);

            builder.Property(x => x.ConfirmationNumber)
                   .HasMaxLength(25).IsRequired();
            builder.HasIndex(x => x.ConfirmationNumber).IsUnique();

            builder.Property(x => x.AppointmentDate)
                   .HasColumnType("date").IsRequired();

            builder.Property(x => x.StartTime)
                   .HasColumnType("time").IsRequired();

            builder.Property(x => x.EndTime)
                   .HasColumnType("time").IsRequired();

            // Composite index — critical for slot conflict checking
            builder.HasIndex(x => new { x.DoctorId, x.AppointmentDate })
                   .HasDatabaseName("IX_Appointments_DoctorId_Date");

            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.DoctorId);

            builder.HasOne(x => x.Patient)
                   .WithMany()
                   .HasForeignKey(x => x.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                   .WithMany()
                   .HasForeignKey(x => x.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
