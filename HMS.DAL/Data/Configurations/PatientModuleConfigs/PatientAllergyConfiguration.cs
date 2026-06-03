using HMS.DAL.Models.PatientModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.PatientModuleConfigs
{
    public class PatientAllergyConfiguration : IEntityTypeConfiguration<PatientAllergy>
    {
        public void Configure(EntityTypeBuilder<PatientAllergy> builder)
        {
            builder.ToTable("PatientAllergies");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                   .HasConversion<string>()
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Severity)
                   .HasConversion<string>()
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Description)
                   .HasMaxLength(1000)
                   .IsRequired();

            builder.Property(x => x.RecordDate)
                   .IsRequired();

            builder.Property(x => x.RecordedBy)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasOne(x => x.Patient)
                   .WithMany(p => p.PatientAllergies)
                   .HasForeignKey(x => x.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
