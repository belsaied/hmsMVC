using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class InvoicesByPatientSpecification : BaseSpecifications<Invoice,Guid>
    {
        public InvoicesByPatientSpecification(int patientId) : base(i => i.PatientId == patientId)
        {
            AddInclude(i => i.LineItems);
            AddInclude(i => i.Payments);
            AddOrderByDescending(i => i.CreatedAt);
        }
    }
}
