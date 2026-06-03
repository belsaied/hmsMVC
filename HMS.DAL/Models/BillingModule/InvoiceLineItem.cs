using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.DAL.Models.BillingModule
{
    public class InvoiceLineItem : BaseEntity<Guid>
    {
        public InvoiceLineItem()
        {
            Id = Guid.NewGuid();
        }

        public Guid InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public LineItemType LineItemType { get; set; }

        public string? ReferenceId { get; set; }

        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;

        public void RecalculateTotal() => Total = Quantity * UnitPrice;
    }
}
