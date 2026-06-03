using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.BillingConfigs
{
    public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
        {
            builder.HasKey(li => li.Id);

            builder.Property(li => li.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(li => li.LineItemType)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<LineItemType>(t))
                .HasMaxLength(20);

            builder.Property(li => li.UnitPrice).HasColumnType("decimal(18,4)");
            builder.Property(li => li.Total).HasColumnType("decimal(18,4)");

            builder.ToTable("InvoiceLineItems");
        }
    }
}
