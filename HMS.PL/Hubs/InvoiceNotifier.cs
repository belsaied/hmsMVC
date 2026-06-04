using HMS.BLL.ServicesAbstraction.Contracts.BillingService;

namespace HMS.PL.Hubs
{
    public class InvoiceNotifier : IInvoiceNotifier
    {
        public Task NotifyInvoiceDueSoonAsync(int patientId, Guid invoiceId,
        string invoiceNumber, decimal outstandingBalance, DateOnly dueDate)
       => Task.CompletedTask;
    }
}
