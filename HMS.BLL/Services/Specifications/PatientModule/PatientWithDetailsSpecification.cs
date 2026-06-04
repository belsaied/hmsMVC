using HMS.DAL.Models.PatientModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.PatientModule
{
    public class PatientWithDetailsSpecification : BaseSpecifications<Patient, int>
    {
        // Get single patient with all related data
        public PatientWithDetailsSpecification(int id)
            : base(p => p.Id == id)
        {
            AddInclude(p => p.PatientAllergies);
            AddInclude(p => p.PatientMedicalHistories);
            AddInclude(p => p.EmergencyContacts);
        }

        // Get all patients with pagination
        public PatientWithDetailsSpecification(PatientSpecificationParameters parameters)
            : base(p =>
                (string.IsNullOrEmpty(parameters.Search) ||
                 p.FirstName.ToLower().Contains(parameters.Search.ToLower()) ||
                 p.LastName.ToLower().Contains(parameters.Search.ToLower())) &&
                (!parameters.Status.HasValue || p.Status == parameters.Status.Value))
        {
            AddInclude(p => p.PatientAllergies);
            AddInclude(p => p.PatientMedicalHistories);
            AddInclude(p => p.EmergencyContacts);
            AddOrderBy(p => p.LastName);
            ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }
    }
}
