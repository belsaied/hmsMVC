using HMS.DAL.Models.PatientModule;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientEmergencyContactSpecification : BaseSpecifications<EmergencyContact,int>
    {
        public PatientEmergencyContactSpecification(int patientId)
    : base(c => c.PatientId == patientId)
        {
        }
    }
}
