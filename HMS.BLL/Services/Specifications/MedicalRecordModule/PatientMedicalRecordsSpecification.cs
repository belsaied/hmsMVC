using HMS.DAL.Models.MedicalRecordModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class PatientMedicalRecordsSpecification : BaseSpecifications<MedicalRecord, int>
    {
        public PatientMedicalRecordsSpecification(int patientId, MedicalRecordSpecificationParameters p) : base(r => r.PatientId == patientId)
        {
            AddInclude(r => r.Doctor);
            AddInclude(r => r.VitalSigns);
            AddInclude(r => r.Prescriptions);
            AddInclude(r => r.LabOrders);
            AddOrderByDescending(r => r.VisitDate);
            ApplyPagination(p.PageSize, p.PageIndex);
        }
    }
}
