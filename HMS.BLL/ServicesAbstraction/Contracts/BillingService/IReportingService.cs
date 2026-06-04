using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.ServicesAbstraction.Contracts.BillingService
{
    public interface IReportingService
    {
        Task<RevenueReportResultDto> GetRevenueReportAsync(ReportFilterParameters filters);
        Task<IEnumerable<InvoiceSummaryResultDto>> GetOutstandingInvoicesReportAsync();
        Task<byte[]> ExportRevenueToExcelAsync(ReportFilterParameters filters);
    }
}
