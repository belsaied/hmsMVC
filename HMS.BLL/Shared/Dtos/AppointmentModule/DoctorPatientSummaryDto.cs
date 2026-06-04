namespace HMS.BLL.Shared.Dtos.AppointmentModule
{
    public record DoctorPatientSummaryDto
    {
        public int PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Gender { get; init; } = string.Empty;
        public int Age { get; init; }
        public string MedicalRecordNumber { get; init; } = string.Empty;
        public IEnumerable<PatientAppointmentSummaryDto> Appointments { get; init; } = [];
    }

    public record PatientAppointmentSummaryDto
    {
        public int AppointmentId { get; init; }
        public string ConfirmationNumber { get; init; } = string.Empty;
        public DateOnly AppointmentDate { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string ReasonForVisit { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public string? CancellationReason { get; init; }
        public DateTime BookedAt { get; init; }
    }
}
