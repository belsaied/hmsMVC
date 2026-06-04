using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record PaymentResultDto
    {
        public Guid Id { get; init; }
        public Guid InvoiceId { get; init; }
        public decimal Amount { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public PaymentStatus Status { get; init; }
        public string? StripeClientSecret { get; init; }
        public string? TransactionReference { get; init; }
        public DateTimeOffset? PaidAt { get; init; }
        public DateTimeOffset? RefundedAt { get; init; }
        public string? RefundReason { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
