namespace HMS.BLL.Shared.Dtos.PatientModule.Medical_History_Dtos
{
    public record MedicalHistoryResultDto
    {
        public int Id { get; init; }
        public int PatientId { get; init; }
        public string ConditionType { get; init; } = string.Empty;
        public string Diagnosis { get; init; } = string.Empty;
        public DateTime DiagnosisDate { get; init; }
        public string? Treatment { get; init; }
        public DateTime? TreatmentStartDate { get; init; }
        public DateTime? ResolutionDate { get; init; }
        public bool IsActive { get; init; }
        public string? RecordedBy { get; init; }
        public DateTime RecordedDate { get; init; }
        public string? Notes { get; init; }
    }
}
