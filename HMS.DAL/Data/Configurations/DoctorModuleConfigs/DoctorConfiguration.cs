using HMS.DAL.Models.DoctorModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.DoctorModuleConfigs
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.NationalId)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.NationalId)
                .IsUnique();

            builder.Property(x => x.LicenseNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.LicenseNumber)
                .IsUnique();

            builder.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.Phone)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Specialization)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.YearsOfExperience)
                .HasDefaultValue(0);

            builder.Property(x => x.ConsultationFee)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(x => x.PictureUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Bio)
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.HasIndex(x => x.DepartmentId); // لزوم الفلترة لو عايز اجيب الدكاترة اللي في قسم معين او اللي في القسم رقم 3 مثلا 

            //Ignore Computed Property
            builder.Ignore(x => x.Age);
           
            #region RelationShips
            builder.HasOne(x => x.Department)
                .WithMany(d => d.Doctors)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict: لو حذفت قسم عنده دكاترة هيرمي Error بدل ما يحذف الدكاترة
            #endregion

        }
    }
}
