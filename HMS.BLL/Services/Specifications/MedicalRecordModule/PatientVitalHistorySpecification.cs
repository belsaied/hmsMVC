using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class PatientVitalHistorySpecification : BaseSpecifications<VitalSign, int>
    {
        public PatientVitalHistorySpecification(int patientId) : base(v => v.PatientId == patientId)
        {
            AddOrderByDescending(v => v.RecordedAt);
        }
    }
}
