using HMS.DAL.Models.PatientModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.PatientModuleConfigs
{
    public class PatientMedicalHistoryConfiguration : IEntityTypeConfiguration<PatientMedicalHistory>
    {
        public void Configure(EntityTypeBuilder<PatientMedicalHistory> builder)
        {
            builder.ToTable("PatientMedicalHistories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConditionType)
                .HasConversion<string>()   //store as String in Data Base
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.RecordedBy)
                .HasMaxLength(100);
            
            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.Property(x => x.Diagnosis)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Treatment)
                .HasMaxLength(1000);


            // Relationship with Patient
            builder.HasOne(x => x.Patient)
                .WithMany(p => p.PatientMedicalHistories)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
