using HMS.DAL.Models.NotificationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Data.Configurations.NotificationModuleConfigs
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            builder.ToTable("NotificationTemplates");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.NotificationType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.Channel)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(t => t.SubjectTemplate)
                .HasMaxLength(300);

            builder.Property(t => t.BodyTemplate)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true);

            // Unique: one template per type + channel
            builder.HasIndex(t => new { t.NotificationType, t.Channel })
                .IsUnique()
                .HasDatabaseName("IX_NotificationTemplates_Type_Channel");
        }
    }
}
