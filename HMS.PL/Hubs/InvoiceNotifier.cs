using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using Microsoft.AspNetCore.SignalR;

namespace HMS.PL.Hubs
{
    public class InvoiceNotifier(IHubContext<NotificationHub> _hubContext) : IInvoiceNotifier
    {
        public async Task NotifyInvoiceDueSoonAsync(int patientId, Guid invoiceId,
            string invoiceNumber, decimal outstandingBalance, DateOnly dueDate)
        {
            await _hubContext.Clients
                .Group($"patient-{patientId}")
                .SendAsync("InvoiceDueSoon", new
                {
                    invoiceId,
                    invoiceNumber,
                    outstandingBalance,
                    dueDate = dueDate.ToString("yyyy-MM-dd")
                });
        }
    }
}
