using HMS.DAL.Models.PatientModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientCountSpecification: BaseSpecifications<Patient, int>
    {
        public PatientCountSpecification(PatientSpecificationParameters parameters)
    : base(p =>
        (string.IsNullOrEmpty(parameters.Search) ||
         p.FirstName.ToLower().Contains(parameters.Search.ToLower()) ||
         p.LastName.ToLower().Contains(parameters.Search.ToLower())) &&
        (!parameters.Status.HasValue || p.Status == parameters.Status.Value))
        {
        }
    }
}
