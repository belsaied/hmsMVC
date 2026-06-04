using HMS.DAL.Models.BillingModule;

namespace HMS.BLL.ServicesAbstraction.Contracts.BillingService
{
    public interface IInvoicePdfGenerator
    {
        byte[] Generate(Invoice invoice, string patientName, string patientEmail);
    }
}
