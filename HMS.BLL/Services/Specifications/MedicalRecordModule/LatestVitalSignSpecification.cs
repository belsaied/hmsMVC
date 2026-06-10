using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public sealed class LatestVitalSignSpecification : BaseSpecifications<VitalSign, int>
    {
        public LatestVitalSignSpecification(int patientId)
            : base(v => v.PatientId == patientId)
        {
            AddOrderByDescending(v => v.RecordedAt);
            ApplyPagination(1, 1);
        }
    }
}
