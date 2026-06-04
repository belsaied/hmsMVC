using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class ClaimWithInvoiceSpecification : BaseSpecifications<InsuranceClaim,Guid>
    {
        public ClaimWithInvoiceSpecification(Guid claimId)
    : base(c => c.Id == claimId)
        {
            AddInclude(c => c.Invoice);
        }
    }
}
