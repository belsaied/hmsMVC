using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.DAL.Models.BillingModule
{
    public class Invoice : BaseEntity<Guid>
    {
        public Invoice()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
            Status = InvoiceStatus.Draft;
        }

        public string InvoiceNumber { get; set; } = string.Empty;

        // ── Foreign Keys ─────────────────────────────────────────────────
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal OutstandingBalance { get; set; }

        // ── Metadata ─────────────────────────────────────────────────────
        public string? Notes { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // ── Navigation Properties ─────────────────────────────────────────
        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public InsuranceClaim? InsuranceClaim { get; set; }

        // ── Domain Methods ────────────────────────────────────────────────

        public void RecalculateFinancials()
        {
            SubTotal = LineItems.Sum(li => li.Total);

            var discountValue = DiscountAmount + (SubTotal * DiscountPercent / 100m);
            var taxableAmount = SubTotal - discountValue;
            TaxAmount = taxableAmount * TaxPercent / 100m;
            TotalAmount = taxableAmount + TaxAmount;
            OutstandingBalance = TotalAmount - PaidAmount;
        }
    }
}
