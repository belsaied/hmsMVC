using HMS.BLL.Shared.Dtos.BillingModule.Requests;
using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.ServicesAbstraction.Contracts.BillingService
{
    public interface IInsuranceService
    {
        Task<ClaimResultDto> SubmitClaimAsync(SubmitClaimRequest request);
        Task<ClaimResultDto> GetClaimByInvoiceAsync(Guid invoiceId);
        Task<ClaimResultDto> UpdateClaimStatusAsync(Guid claimId, UpdateClaimRequest request);
        Task<ClaimResultDto> ResubmitClaimAsync(Guid claimId, ResubmitClaimRequest request);
    }

}
