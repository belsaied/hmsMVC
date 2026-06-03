using HMS.DAL.Models.WardBedModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.WardBedConfigs
{
    public class WardConfiguration : IEntityTypeConfiguration<Ward>
    {
        public void Configure(EntityTypeBuilder<Ward> builder)
        {
            builder.ToTable("Wards");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasIndex(x => x.Name)
                   .IsUnique();

            builder.Property(x => x.WardType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.PhoneExtension)
                   .HasMaxLength(20);

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);
            
            //chat
            builder.HasMany(x => x.Rooms)
                   .WithOne(x => x.Ward)
                   .HasForeignKey(x => x.WardId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
