using HMS.DAL.Models.Enums.BillingEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.BillingModule
{
    public class UpdateClaimViewModel
    {
        public Guid ClaimId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal ClaimedAmount { get; set; }

        [Required]
        [Display(Name = "New Status")]
        public ClaimStatus ClaimStatus { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Approved Amount")]
        public decimal? ApprovedAmount { get; set; }

        [MaxLength(500)]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }
    }
}
