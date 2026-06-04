namespace HMS.BLL.Shared.Dtos.PatientModule.AllergyDtos
{
    public record AllergyResultDto
    {
        public int Id { get; init; }
        public int PatientId { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime RecordDate { get; init; }
        public string RecordedBy { get; init; } = string.Empty;
    }
}
