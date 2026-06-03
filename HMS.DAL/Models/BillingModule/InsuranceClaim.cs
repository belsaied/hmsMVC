using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.DAL.Models.BillingModule
{
    public class InsuranceClaim : BaseEntity<Guid>
    {
        public InsuranceClaim()
        {
            Id = Guid.NewGuid();
        }

        public Guid InvoiceId { get; set; }
        public int PatientId { get; set; }

        public string InsuranceProvider { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string MembershipNumber { get; set; } = string.Empty;

        public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Submitted;

        public decimal ClaimedAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string? RejectionReason { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }

        public string? ExternalClaimReference { get; set; }

        public string? Notes { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;
    }
}
