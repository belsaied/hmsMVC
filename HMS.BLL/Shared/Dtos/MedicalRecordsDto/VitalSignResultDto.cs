namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record VitalSignResultDto
    {
        public int Id { get; init; }
        public int MedicalRecordId { get; init; }
        public int PatientId { get; init; }
        public decimal? Temperature { get; init; }
        public int? BloodPressureSystolic { get; init; }
        public int? BloodPressureDiastolic { get; init; }
        public int? HeartRate { get; init; }
        public int? RespiratoryRate { get; init; }
        public decimal? OxygenSaturation { get; init; }
        public decimal? Weight { get; init; }
        public decimal? Height { get; init; }
        public DateTime RecordedAt { get; init; }
        public string RecordedBy { get; init; } = string.Empty;
    }
}
