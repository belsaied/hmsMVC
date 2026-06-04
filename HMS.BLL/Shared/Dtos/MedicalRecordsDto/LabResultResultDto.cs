namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record LabResultResultDto
    {
        public int Id { get; init; }
        public int LabOrderId { get; init; }
        public string ResultText { get; init; } = string.Empty;
        public bool IsAbnormal { get; init; }
        public string? NormalRange { get; init; }
        public DateTime PerformedAt { get; init; }
        public DateTime ReportedAt { get; init; }
        public string PerformedBy { get; init; } = string.Empty;
    }
}
