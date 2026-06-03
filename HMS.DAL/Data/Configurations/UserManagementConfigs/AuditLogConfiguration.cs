using HMS.DAL.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.UserManagementConfigs
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .HasMaxLength(450)   // matches AspNetUsers PK size
                .IsRequired();

            builder.Property(x => x.Action)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Details)
                .HasMaxLength(2000);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45);   // supports IPv6

            builder.Property(x => x.CreatedAt)  
                .HasColumnType("datetimeoffset")
                .IsRequired();

            // Indexes for common queries (audit trail by user, audit trail by action)
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(x => new { x.UserId, x.CreatedAt })  
                .HasDatabaseName("IX_AuditLogs_UserId_OccurredAt");

        }
    }
}
