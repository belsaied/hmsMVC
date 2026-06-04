using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class InvoicesByStatusSpecification : BaseSpecifications<Invoice,Guid>
    {
        public InvoicesByStatusSpecification(IEnumerable<InvoiceStatus> statuses)
    : base(i => statuses.Contains(i.Status))
        {
            AddOrderByDescending(i => i.CreatedAt);
        }
    }
}
