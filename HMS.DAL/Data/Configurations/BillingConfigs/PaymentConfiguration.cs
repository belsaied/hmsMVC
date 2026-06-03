using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.BillingConfigs
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount).HasColumnType("decimal(18,4)");

            builder.Property(p => p.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<PaymentStatus>(s))
                .HasMaxLength(20);

            builder.Property(p => p.PaymentMethod)
                .HasConversion(
                    m => m.ToString(),
                    m => Enum.Parse<PaymentMethod>(m))
                .HasMaxLength(20);

            // Nullable unique index on StripePaymentIntentId
            // (nullable fields can share index with NULL values in SQL Server)
            builder.HasIndex(p => p.StripePaymentIntentId)
                .IsUnique()
                .HasFilter("[StripePaymentIntentId] IS NOT NULL");

            builder.Property(p => p.StripePaymentIntentId).HasMaxLength(255);
            builder.Property(p => p.StripeClientSecret).HasMaxLength(500);
            builder.Property(p => p.TransactionReference).HasMaxLength(100);
            builder.Property(p => p.FailureReason).HasMaxLength(500);
            builder.Property(p => p.RefundReason).HasMaxLength(500);

            builder.ToTable("Payments");
        }
    }
}
