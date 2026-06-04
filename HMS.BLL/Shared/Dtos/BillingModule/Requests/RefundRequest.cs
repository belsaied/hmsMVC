using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record RefundRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required]
        [MinLength(3)]
        [MaxLength(500)]
        public string Reason { get; init; } = string.Empty;
    }
}
