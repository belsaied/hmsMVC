namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record RevenueByGroupResultDto
    {
        public string Label { get; init; } = string.Empty;
        public decimal TotalRevenue { get; init; }
        public int InvoiceCount { get; init; }
    }
}
