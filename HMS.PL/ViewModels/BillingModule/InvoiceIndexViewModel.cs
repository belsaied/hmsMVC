using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.PL.ViewModels.BillingModule
{
    public class InvoiceIndexViewModel
    {
        public IEnumerable<InvoiceSummaryResultDto> Invoices { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public InvoiceStatus? StatusFilter { get; set; }
        public int? PatientIdFilter { get; set; }
    }
}
