namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record LabOrderResultDto
    {
        public int Id { get; init; }
        public int MedicalRecordId { get; init; }
        public int PatientId { get; init; }
        public int DoctorId { get; init; }
        public string TestName { get; init; } = string.Empty;
        public string? TestCode { get; init; }
        public string Priority { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime OrderedAt { get; init; }
        public string? Notes { get; init; }
        public LabResultResultDto? Result { get; init; }
    }
}
