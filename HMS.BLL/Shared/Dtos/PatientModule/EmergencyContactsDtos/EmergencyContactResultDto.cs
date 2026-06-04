namespace HMS.BLL.Shared.Dtos.PatientModule.EmergencyContactsDtos
{
    public record EmergencyContactResultDto
    {
        public int Id { get; init; }
        public int PatientId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Relationship { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? Email { get; init; }
        public bool IsPrimaryContact { get; init; }
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
