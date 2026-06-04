using HMS.DAL.Models.PatientModule;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientByEmailSpecification : BaseSpecifications<Patient,int>
    {
        public PatientByEmailSpecification(string email)
    : base(p => p.Email.ToLower() == email.ToLower()) { }
    }
}
