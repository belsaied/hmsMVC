using HMS.DAL.Models.DoctorModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.DoctorModuleConfigs
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Department");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x=>x.PhoneExtension)
                   .HasMaxLength (20);

            #region Relationships
            builder.HasOne(x => x.HeadDoctor)
                   .WithMany()
                   .HasForeignKey(x => x.HeadDoctorId)
                   .OnDelete(DeleteBehavior.SetNull)  // OnDelete = SetNull لأن لو الدكتور اتمسح، القسم مش هيتمسح معاه
                   .IsRequired(false); 
            #endregion
        }
    }
}
