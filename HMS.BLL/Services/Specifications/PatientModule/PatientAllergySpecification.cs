using HMS.DAL.Models.PatientModule;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientAllergySpecification : BaseSpecifications<PatientAllergy,int>
    {
        public PatientAllergySpecification(int patientId)
         : base(a => a.PatientId == patientId)
        {
        }
    }
}
