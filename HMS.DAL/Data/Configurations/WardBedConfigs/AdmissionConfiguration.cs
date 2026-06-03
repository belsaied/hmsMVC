using HMS.DAL.Models.WardBedModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.WardBedConfigs
{
    public class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
    {
        public void Configure(EntityTypeBuilder<Admission> builder)
        {
            builder.ToTable("Admissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AdmissionDate)
                   .HasColumnType("datetime2")
                   .IsRequired();

            builder.Property(x => x.ExpectedDischargeDate)
                   .HasColumnType("date");

            builder.Property(x => x.ActualDischargeDate)
                   .HasColumnType("datetime2");

            builder.Property(x => x.AdmissionReason)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(x => x.DischargeSummary)
                   .HasMaxLength(2000);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.AdmittedBy)
                   .HasMaxLength(100)
                   .IsRequired();

            // Indexes for common queries
            builder.HasIndex(x => x.PatientId)
                   .HasDatabaseName("IX_Admissions_PatientId");

            builder.HasIndex(x => x.BedId)
                   .HasDatabaseName("IX_Admissions_BedId");

            builder.HasIndex(x => x.AdmittingDoctorId)
                   .HasDatabaseName("IX_Admissions_DoctorId");

            #region Relationships
            builder.HasOne(x => x.Patient)
                   .WithMany()
                   .HasForeignKey(x => x.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Bed)
                   .WithMany(x=>x.Admissions)//=>Chat
                   .HasForeignKey(x => x.BedId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AdmittingDoctor)
                   .WithMany()
                   .HasForeignKey(x => x.AdmittingDoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
            //chat
            builder.HasMany(x => x.Transfers)
                   .WithOne(x => x.Admission)
                   .HasForeignKey(x => x.AdmissionId)
                   .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
