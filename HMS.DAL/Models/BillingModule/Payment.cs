using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.DAL.Models.BillingModule
{
    public class Payment : BaseEntity<Guid>
    {
        public Payment()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
            Status = PaymentStatus.Pending;
        }

        public Guid InvoiceId { get; set; }

        public int PatientId { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }

        // ── Stripe ───────────────────────────────────────────────────────
        public string? StripePaymentIntentId { get; set; }

        public string? StripeClientSecret { get; set; }

        // ── Non-Stripe ───────────────────────────────────────────────────
        public string? TransactionReference { get; set; }

        // ── Status Metadata ───────────────────────────────────────────────
        public string? FailureReason { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? RefundedAt { get; set; }
        public string? RefundReason { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;
    }
}
