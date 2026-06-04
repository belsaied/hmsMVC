using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record ClaimResultDto
    {
        public Guid Id { get; init; }
        public Guid InvoiceId { get; init; }
        public string InsuranceProvider { get; init; } = string.Empty;
        public string PolicyNumber { get; init; } = string.Empty;
        public string MembershipNumber { get; init; } = string.Empty;
        public ClaimStatus ClaimStatus { get; init; }
        public decimal ClaimedAmount { get; init; }
        public decimal ApprovedAmount { get; init; }
        public decimal PatientCopayment { get; init; }
        public string? RejectionReason { get; init; }
        public string? ExternalClaimReference { get; init; }
        public string? Notes { get; init; }
        public DateTimeOffset? SubmittedAt { get; init; }
        public DateTimeOffset? ResolvedAt { get; init; }
    }
}
