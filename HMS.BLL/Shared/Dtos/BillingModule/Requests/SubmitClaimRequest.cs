using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record SubmitClaimRequest
    {
        [Required]
        public Guid InvoiceId { get; init; }

        [Required]
        [MaxLength(200)]
        public string InsuranceProvider { get; init; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; init; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string MembershipNumber { get; init; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ClaimedAmount { get; init; }

        [MaxLength(1000)]
        public string? Notes { get; init; }
    }
}
