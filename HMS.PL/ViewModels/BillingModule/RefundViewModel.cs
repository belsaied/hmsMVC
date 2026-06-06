using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.BillingModule
{
    public class RefundViewModel
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal PaidAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required, MinLength(3), MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
