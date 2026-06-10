using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class InvoicesByNumberSpecification : BaseSpecifications<Invoice, Guid>
    {
        public InvoicesByNumberSpecification(string invoiceNumber)
            : base(i => i.InvoiceNumber == invoiceNumber)
        { }
    }
}
