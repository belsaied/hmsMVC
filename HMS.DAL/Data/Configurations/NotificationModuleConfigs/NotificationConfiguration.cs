using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.NotificationModuleConfigs
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.RecipientUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(n => n.RecipientType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.RecipientEmail)
                .HasMaxLength(256);

            builder.Property(n => n.RecipientPhone)
                .HasMaxLength(30);

            builder.Property(n => n.Channel)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.NotificationType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(n => n.Subject)
                .HasMaxLength(300);

            builder.Property(n => n.Body)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(n => n.DeliveryStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.FailureReason)
                .HasMaxLength(500);

            builder.Property(n => n.ExternalMessageId)
                .HasMaxLength(200);

            builder.Property(n => n.RelatedEntityId)
                .HasMaxLength(100);

            builder.Property(n => n.RelatedEntityType)
                .HasMaxLength(50);

            // Index for user log queries (most common query pattern)
            builder.HasIndex(n => new { n.RecipientUserId, n.CreatedAt })
                .HasDatabaseName("IX_Notifications_UserId_CreatedAt");

            // Index for deduplication queries used by Hangfire jobs
            builder.HasIndex(n => new { n.RelatedEntityId, n.NotificationType })
                .HasDatabaseName("IX_Notifications_RelatedEntityId_Type");

            // Index for unread push queries
            builder.HasIndex(n => new { n.RecipientUserId, n.Channel, n.IsRead })
                .HasDatabaseName("IX_Notifications_UserId_Channel_IsRead");
        }
    }
}
