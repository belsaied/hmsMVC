using HMS.DAL.Models.MedicalRecordModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class DoctorMedicalRecordsSpecification : BaseSpecifications<MedicalRecord, int>
    {
        public DoctorMedicalRecordsSpecification(int doctorId , MedicalRecordSpecificationParameters p) 
            : base(r => r.DoctorId == doctorId)
        {
            AddInclude(r => r.Patient);
            AddOrderByDescending(r => r.VisitDate);
            ApplyPagination(p.PageSize, p.PageIndex);
        }
    }
}
