using HMS.DAL.Models.DoctorModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.DoctorModuleConfigs
{
    public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
        {
            builder.ToTable("DoctorSchedules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DayOfWeek)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.StartTime)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(x => x.EndTime)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(x => x.SlotDurationMinutes)
                .HasDefaultValue(30)
                .IsRequired();

            builder.Property(x => x.IsAvailable)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.MaxAppointmentsPerSlot)
                .HasDefaultValue(1)
                .IsRequired();

            builder.HasIndex(x => x.DoctorId);

            #region RelationShips
            builder.HasOne(x => x.Doctor)
                    .WithMany(d => d.AvailabilitySchedules)
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade); 
            #endregion
        }
    }
}
