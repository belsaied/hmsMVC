using HMS.DAL.Models.PatientModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientListSpecification : BaseSpecifications<Patient,int>
    {
        public PatientListSpecification(PatientSpecificationParameters parameters)
    : base(p =>
        (string.IsNullOrEmpty(parameters.Search) ||
         p.FirstName.ToLower().Contains(parameters.Search.ToLower()) ||
         p.LastName.ToLower().Contains(parameters.Search.ToLower())) &&
        (!parameters.Status.HasValue || p.Status == parameters.Status.Value))
        {
            AddOrderBy(p => p.Id);
            ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }
    }
}
