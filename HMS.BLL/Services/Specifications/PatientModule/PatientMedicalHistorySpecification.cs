using HMS.DAL.Models.PatientModule;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientMedicalHistorySpecification : BaseSpecifications<PatientMedicalHistory, int>
    {
        public PatientMedicalHistorySpecification(int patientId)
    : base(h => h.PatientId == patientId)
        {
        }
    }
}
