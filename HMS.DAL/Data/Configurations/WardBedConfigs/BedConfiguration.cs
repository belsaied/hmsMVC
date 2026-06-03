using HMS.DAL.Models.WardBedModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.WardBedConfigs
{
    public class BedConfiguration : IEntityTypeConfiguration<Bed>
    {
        public void Configure(EntityTypeBuilder<Bed> builder)
        {
            builder.ToTable("Beds");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BedNumber)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.BedType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.Notes)
                   .HasMaxLength(500);

            builder.HasIndex(x => new { x.RoomId, x.BedNumber })
                   .IsUnique()
                   .HasDatabaseName("IX_Beds_RoomId_BedNumber");

            #region Relationships
            builder.HasOne(x => x.Room)
                   .WithMany(r => r.Beds)
                   .HasForeignKey(x => x.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Admissions)
                   .WithOne(x => x.Bed)
                   .HasForeignKey(x => x.BedId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
