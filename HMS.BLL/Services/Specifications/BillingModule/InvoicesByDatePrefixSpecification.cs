using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.Services.Specifications.BillingModule
{
    public sealed class InvoicesByDatePrefixSpecification : BaseSpecifications<Invoice,Guid>
    {
        public InvoicesByDatePrefixSpecification(string dateSegment)
    : base(i => i.InvoiceNumber.StartsWith($"INV-{dateSegment}-"))
        { }
    }
}
