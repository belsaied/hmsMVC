using HMS.DAL.Models.WardBedModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.WardBedConfigs
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RoomNumber)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.RoomType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.Capacity)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.Property(x => x.Notes)
                   .HasMaxLength(500);

            // Composite unique index — RoomNumber must be unique within a Ward
            builder.HasIndex(x => new { x.WardId, x.RoomNumber })
                   .IsUnique()
                   .HasDatabaseName("IX_Rooms_WardId_RoomNumber");

            #region Relationships
            builder.HasOne(x => x.Ward)
                   .WithMany(w => w.Rooms)
                   .HasForeignKey(x => x.WardId)
                   .OnDelete(DeleteBehavior.Restrict);

            //chat
            builder.HasMany(x => x.Beds)
                   .WithOne(x => x.Room)
                   .HasForeignKey(x => x.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
