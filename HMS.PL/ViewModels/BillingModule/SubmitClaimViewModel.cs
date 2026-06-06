using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.BillingModule
{
    public class SubmitClaimViewModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Insurance Provider")]
        public string InsuranceProvider { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Membership Number")]
        public string MembershipNumber { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        [Display(Name = "Claimed Amount")]
        public decimal ClaimedAmount { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
