using HMS.DAL.Models.BillingModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class InvoiceListSpecification : BaseSpecifications<Invoice, Guid>
    {
        public InvoiceListSpecification(InvoiceFilterParameters p)
    : base(i =>
        (!p.Status.HasValue || i.Status == p.Status.Value) &&
        (!p.PatientId.HasValue || i.PatientId == p.PatientId.Value))
        {
            AddOrderByDescending(i => i.CreatedAt);
            ApplyPagination(p.PageSize, p.PageIndex);
        }
    }
}
