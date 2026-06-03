using HMS.DAL.Models.PatientModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.PatientModuleConfigs
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FirstName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.LastName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.NationalId)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.HasIndex(p => p.NationalId)
                   .IsUnique();

            builder.Property(p => p.MedicalRecordNumber)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.HasIndex(p => p.MedicalRecordNumber)
                   .IsUnique();

            builder.Property(p => p.Phone)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(p => p.Email)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasIndex(p => p.Email)
                   .IsUnique();

            builder.Property(p => p.Gender)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(p => p.BloodType)
                   .HasConversion<string>()
                   .HasMaxLength(10);

            builder.Property(p => p.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(p => p.PictureUrl)
                   .HasMaxLength(500);

            builder.OwnsOne(p => p.Address, address =>
            {
                address.Property(a => a.Street)
                       .HasMaxLength(200);

                address.Property(a => a.City)
                       .HasMaxLength(100);

                address.Property(a => a.Country)
                       .HasMaxLength(100);

                address.Property(a => a.PostalCode)
                       .HasMaxLength(20);
            });
            // Computed column (optional, not mapped)
            builder.Ignore(p => p.Age);
        }
    }
}
