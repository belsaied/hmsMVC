using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class MedicalRecordWithDetailsSpecification : BaseSpecifications<MedicalRecord,int>
    {
        public MedicalRecordWithDetailsSpecification(int id) : base(r => r.Id == id)
        {
            AddInclude(r => r.Patient);
            AddInclude(r => r.Doctor);
            AddInclude(r => r.VitalSigns);
            AddInclude(r => r.Prescriptions);
            AddInclude(r => r.LabOrders);
        }
    }
}
