namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record MedicalRecordResultDto
    {
        public int Id { get; init; }
        public int PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public int DoctorId { get; init; }
        public string DoctorName { get; init; } = string.Empty;
        public int? AppointmentId { get; init; }
        public DateTime VisitDate { get; init; }
        public DateTime RecordedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string ChiefComplaint { get; init; } = string.Empty;
        public string Diagnosis { get; init; } = string.Empty;
        public string? IcdCode { get; init; }
        public string? ClinicalNotes { get; init; }
        public string? TreatmentPlan { get; init; }
        public DateOnly? FollowUpDate { get; init; }
        public bool IsConfidential { get; init; }
        public IEnumerable<VitalSignResultDto> VitalSigns { get; init; } = [];
        public IEnumerable<PrescriptionResultDto> Prescriptions { get; init; } = [];
        public IEnumerable<LabOrderResultDto> LabOrders { get; init; } = [];
    }
}
