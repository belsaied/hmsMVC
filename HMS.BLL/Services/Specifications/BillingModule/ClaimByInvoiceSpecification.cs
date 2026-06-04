using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class ClaimByInvoiceSpecification : BaseSpecifications<InsuranceClaim,Guid>
    {
        public ClaimByInvoiceSpecification(Guid invoiceId)
    : base(c => c.InvoiceId == invoiceId)
        {
            AddInclude(c => c.Invoice);
        }
    }
}
