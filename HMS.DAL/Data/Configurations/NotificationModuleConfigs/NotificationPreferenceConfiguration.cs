using HMS.DAL.Models.NotificationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.NotificationModuleConfigs
{
    public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.ToTable("NotificationPreferences");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.RecipientType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.NotificationType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Channel)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.IsEnabled)
                .HasDefaultValue(true);

            // Unique: one preference row per user + type + channel combination
            builder.HasIndex(p => new { p.UserId, p.NotificationType, p.Channel })
                .IsUnique()
                .HasDatabaseName("IX_NotificationPreferences_UserId_Type_Channel");
        }
    }
}
