using HMS.DAL.Models.WardBedModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.WardBedConfigs
{
    public class BedTransferConfiguration : IEntityTypeConfiguration<BedTransfer>
    {
        public void Configure(EntityTypeBuilder<BedTransfer> builder)
        {
            builder.ToTable("BedTransfers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransferredAt)
                   .HasColumnType("datetime2")
                   .IsRequired();

            builder.Property(x => x.Reason)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(x => x.TransferredBy)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasIndex(x => x.AdmissionId)
                   .HasDatabaseName("IX_BedTransfers_AdmissionId");

            #region Relationships
            builder.HasOne(x => x.Admission)
                   .WithMany(a => a.Transfers)
                   .HasForeignKey(x => x.AdmissionId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Two FK to Beds — must use NoAction to avoid multiple cascade paths
            builder.HasOne(x => x.FromBed)
                   .WithMany()
                   .HasForeignKey(x => x.FromBedId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ToBed)
                   .WithMany()
                   .HasForeignKey(x => x.ToBedId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
