using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record RecordCashPaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }
    }
}
