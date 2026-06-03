using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.BillingConfigs
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(i => i.InvoiceNumber).IsUnique();

            builder.Property(i => i.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<InvoiceStatus>(s))
                .HasMaxLength(20);

            builder.Property(i => i.SubTotal).HasColumnType("decimal(18,4)");
            builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,4)");
            builder.Property(i => i.DiscountPercent).HasColumnType("decimal(5,2)");
            builder.Property(i => i.TaxPercent).HasColumnType("decimal(5,2)");
            builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,4)");
            builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,4)");
            builder.Property(i => i.PaidAmount).HasColumnType("decimal(18,4)");
            builder.Property(i => i.OutstandingBalance).HasColumnType("decimal(18,4)");

            builder.Property(i => i.Notes).HasMaxLength(500);


            builder.HasMany(i => i.LineItems)
                .WithOne(li => li.Invoice)
                .HasForeignKey(li => li.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(i => i.Payments)
                .WithOne(p => p.Invoice)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.InsuranceClaim)
                .WithOne(c => c.Invoice)
                .HasForeignKey<InsuranceClaim>(c => c.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Invoices");
        }
    }
}
