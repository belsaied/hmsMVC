using HMS.BLL.Shared.Dtos.BillingModule.Results;

namespace HMS.BLL.ServicesAbstraction.Contracts.BillingService
{
    public interface IPaymentService
    {
        Task<PaymentIntentResultDto> CreatePaymentIntentAsync(Guid invoiceId);
        Task<PaymentResultDto> RecordCashPaymentAsync(Guid invoiceId, decimal amount);
        Task HandleStripeWebhookAsync(string payload, string stripeSignature);
        Task<PaymentResultDto> RefundPaymentAsync(Guid paymentId, decimal amount, string reason);
        Task<IEnumerable<PaymentResultDto>> GetPaymentsByInvoiceAsync(Guid invoiceId);
    }
}
