using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.BillingModule.Requests;
using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.ServicesAbstraction.Contracts.BillingService
{
    public interface IInvoiceService
    {
        Task<InvoiceResultDto> CreateInvoiceAsync(CreateInvoiceRequest request);
        Task<InvoiceResultDto> GetInvoiceByIdAsync(Guid id);
        Task<IEnumerable<InvoiceSummaryResultDto>> GetInvoicesByPatientAsync(int patientId);
        Task<PaginatedResult<InvoiceSummaryResultDto>> GetAllInvoicesAsync(InvoiceFilterParameters filters);
        Task<InvoiceResultDto> AddLineItemAsync(Guid invoiceId, AddLineItemRequest request);
        Task RemoveLineItemAsync(Guid invoiceId, Guid lineItemId);
        Task<InvoiceResultDto> IssueInvoiceAsync(Guid invoiceId, IssueInvoiceRequest request);
        Task CancelInvoiceAsync(Guid invoiceId, string reason);
        Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId);
    }
}
