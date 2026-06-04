using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record CancelInvoiceRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(500)]
        public string Reason { get; init; } = string.Empty;
    }
}
