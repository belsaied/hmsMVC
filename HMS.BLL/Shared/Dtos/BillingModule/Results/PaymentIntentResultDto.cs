namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record PaymentIntentResultDto
    {
        public Guid InvoiceId { get; init; }
        public string StripePaymentIntentId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
