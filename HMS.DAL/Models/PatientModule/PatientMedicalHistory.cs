using HMS.DAL.Models.Enums.PatientEnums;

namespace HMS.DAL.Models.PatientModule
{
    public class PatientMedicalHistory :BaseEntity<int>
    {
        public int PatientId { get; set; }
        public ConditionType ConditionType { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime DiagnosisDate { get; set; }
        public string? Treatment { get; set; }
        public DateTime? TreatmentStartDate { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? RecordedBy { get; set; }
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
       
        #region Navigation Property
        public Patient Patient { get; set; } = null!;

        #endregion 
    }
}
