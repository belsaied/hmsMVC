using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.BillingModule
{
    public class CancelInvoiceViewModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
