using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class ActivePrescriptionsSpecification : BaseSpecifications<Prescription, int>
    {
        public ActivePrescriptionsSpecification(int patientId)
    : base(p => p.PatientId == patientId && p.Status == PrescriptionStatus.Active)
        {
            AddOrderByDescending(p => p.PrescribedAt);
        }
    }
}
