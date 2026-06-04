using HMS.DAL.Contracts;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using Microsoft.Extensions.Logging;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.Services.Specifications.BillingModule;

namespace HMS.BLL.Services.Implementations.BillingModule
{
    public class BillingBackgroundJobs
    {

    }

    public sealed class MarkOverdueInvoicesJob(
    IUnitOfWork _unitOfWork,
    ILogger<MarkOverdueInvoicesJob> _logger)
    {
        public async Task ExecuteAsync()
        {
            var spec = new OverdueInvoicesSpecification();
            var repo = _unitOfWork.GetRepository<Invoice, Guid>();
            var overdueInvoices = (await repo.GetAllAsync(spec)).ToList();

            if (!overdueInvoices.Any())
            {
                _logger.LogInformation("[MarkOverdueInvoicesJob] No overdue invoices found at {Time}.",
                    DateTime.UtcNow);
                return;
            }

            foreach (var invoice in overdueInvoices)
            {
                invoice.Status = InvoiceStatus.Overdue;
                repo.Update(invoice);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "[MarkOverdueInvoicesJob] Marked {Count} invoice(s) as Overdue at {Time}.",
                overdueInvoices.Count, DateTime.UtcNow);
        }
    }

    public sealed class InvoiceExpiryNotificationJob(
    IUnitOfWork _unitOfWork,
    IInvoiceNotifier _notifier,
    ILogger<InvoiceExpiryNotificationJob> _logger)
    {
        private const int DaysAhead = 3;

        public async Task ExecuteAsync()
        {
            var spec = new InvoicesDueSoonSpecification(DaysAhead);
            var dueSoon = (await _unitOfWork.GetRepository<Invoice, Guid>().GetAllAsync(spec)).ToList();

            if (!dueSoon.Any())
            {
                _logger.LogInformation("[InvoiceExpiryNotificationJob] No invoices due in {Days} days at {Time}.",
                    DaysAhead, DateTime.UtcNow);
                return;
            }

            foreach (var invoice in dueSoon)
            {
                await _notifier.NotifyInvoiceDueSoonAsync(invoice.PatientId, invoice.Id,
                    invoice.InvoiceNumber, invoice.OutstandingBalance, invoice.DueDate!.Value);
            }

            _logger.LogInformation(
                "[InvoiceExpiryNotificationJob] Sent due-soon reminders for {Count} invoice(s) at {Time}.",
                dueSoon.Count, DateTime.UtcNow);
        }
    }


}
