using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class PaymentsByInvoiceSpecification : BaseSpecifications<Payment,Guid>
    {
        public PaymentsByInvoiceSpecification(Guid invoiceId)
    : base(p => p.InvoiceId == invoiceId)
        {
            AddOrderByDescending(p => p.CreatedAt);
        }
    }
}
