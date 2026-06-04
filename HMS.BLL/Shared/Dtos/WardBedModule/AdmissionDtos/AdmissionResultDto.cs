using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos
{
    public record AdmissionResultDto
    {
        public int Id { get; init; }
        public int PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public int BedId { get; init; }
        public string BedNumber { get; init; } = string.Empty;
        public string RoomNumber { get; init; } = string.Empty;
        public string WardName { get; init; } = string.Empty;
        public int AdmittingDoctorId { get; init; }
        public string DoctorName { get; init; } = string.Empty;
        public DateTime AdmissionDate { get; init; }
        public DateOnly? ExpectedDischargeDate { get; init; }
        public DateTime? ActualDischargeDate { get; init; }
        public string AdmissionReason { get; init; } = string.Empty;
        public string? DischargeSummary { get; init; }
        public string Status { get; init; } = string.Empty;
        public string AdmittedBy { get; init; } = string.Empty;
        public IEnumerable<BedTransferResultDto> Transfers { get; init; } = [];
    }
}
