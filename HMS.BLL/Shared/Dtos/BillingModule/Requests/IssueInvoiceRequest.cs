using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record IssueInvoiceRequest
    {
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; init; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; init; }

        [Range(0, 100)]
        public decimal TaxPercent { get; init; }

        [MaxLength(500)]
        public string? Notes { get; init; }
    }
}
