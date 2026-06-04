namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record RevenueReportResultDto
    {
        public string Period { get; init; } = string.Empty;
        public decimal TotalRevenue { get; init; }
        public decimal TotalInvoiced { get; init; }
        public decimal TotalOutstanding { get; init; }
        public List<RevenueByGroupResultDto> ByDoctor { get; init; } = new();
        public List<RevenueByGroupResultDto> ByServiceType { get; init; } = new();
    }
}
