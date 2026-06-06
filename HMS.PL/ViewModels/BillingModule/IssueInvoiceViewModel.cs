using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.BillingModule
{
    public class IssueInvoiceViewModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal CurrentSubTotal { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Discount Amount (flat)")]
        public decimal DiscountAmount { get; set; }

        [Range(0, 100)]
        [Display(Name = "Discount %")]
        public decimal DiscountPercent { get; set; }

        [Range(0, 100)]
        [Display(Name = "Tax %")]
        public decimal TaxPercent { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
