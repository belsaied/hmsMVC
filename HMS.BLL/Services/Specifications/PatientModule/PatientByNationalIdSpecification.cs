using HMS.DAL.Models.PatientModule;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientByNationalIdSpecification : BaseSpecifications<Patient,int>
    {
        public PatientByNationalIdSpecification(string nationalId)
    : base(p => p.NationalId == nationalId) { }
    }
}
