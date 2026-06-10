using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public sealed class PrescriptionsExpiringOnSpecification : BaseSpecifications<Prescription, int>
    {
        public PrescriptionsExpiringOnSpecification(DateOnly expiryDate)
            : base(p => p.Status == PrescriptionStatus.Active && p.ExpiresAt == expiryDate)
        {
        }
    }
}
