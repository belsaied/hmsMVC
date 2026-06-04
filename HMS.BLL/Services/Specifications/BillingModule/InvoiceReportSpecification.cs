using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class InvoiceReportSpecification : BaseSpecifications<Invoice,Guid>
    {
        public InvoiceReportSpecification(DateOnly startDate, DateOnly endDate)
    : base(i =>
        i.CreatedAt >= startDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
        && i.CreatedAt <= endDate.ToDateTime(TimeOnly.MaxValue).ToUniversalTime()
        && i.Status != InvoiceStatus.Cancelled)
        {
            AddInclude(i => i.LineItems);
            AddInclude(i => i.Payments);
        }
    }
}
