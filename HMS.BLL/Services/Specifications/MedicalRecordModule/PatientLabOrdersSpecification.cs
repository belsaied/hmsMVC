using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class PatientLabOrdersSpecification : BaseSpecifications<LabOrder,int>
    {
        public PatientLabOrdersSpecification(int patientId) : base(o => o.PatientId == patientId)
        {
            AddInclude(o => o.Result!);
            AddOrderByDescending(o => o.OrderedAt);
        }
    }
}
