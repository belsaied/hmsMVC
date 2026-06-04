namespace HMS.BLL.ServicesAbstraction.Contracts.BillingService
{
    public interface IInvoiceNotifier
    {
    Task NotifyInvoiceDueSoonAsync(
    int patientId,
    Guid invoiceId,
    string invoiceNumber,
    decimal outstandingBalance,
    DateOnly dueDate);
    }
}
