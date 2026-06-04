using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class PaymentByStripeIntentSpecification : BaseSpecifications<Payment,Guid>
    {
        public PaymentByStripeIntentSpecification(string paymentIntentId)
    : base(p => p.StripePaymentIntentId == paymentIntentId)
        {
            AddInclude(p => p.Invoice);
        }
    }
}
