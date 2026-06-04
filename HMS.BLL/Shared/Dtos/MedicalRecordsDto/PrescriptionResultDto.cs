namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record PrescriptionResultDto
    {
        public int Id { get; init; }
        public int MedicalRecordId { get; init; }
        public int PatientId { get; init; }
        public int DoctorId { get; init; }
        public string MedicationName { get; init; } = string.Empty;
        public string Dosage { get; init; } = string.Empty;
        public string Frequency { get; init; } = string.Empty;
        public int DurationDays { get; init; }
        public string? Instructions { get; init; }
        public bool IsControlledSubstance { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime PrescribedAt { get; init; }
        public DateOnly ExpiresAt { get; init; }
    }
}
