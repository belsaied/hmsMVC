namespace HMS.BLL.Shared.Dtos.AppointmentModule
{
    public record AppointmentResultDto
    {
        public int Id { get; init; }
        public string ConfirmationNumber { get; init; } = string.Empty;
        public int PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public int DoctorId { get; init; }
        public string DoctorName { get; init; } = string.Empty;
        public string DoctorSpecialization { get; init; } = string.Empty;
        public DateOnly AppointmentDate { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string ReasonForVisit { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public string? CancellationReason { get; init; }
        public DateTime BookedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
