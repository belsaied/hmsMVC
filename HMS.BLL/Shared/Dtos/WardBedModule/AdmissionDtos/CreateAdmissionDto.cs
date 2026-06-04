using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos
{
    public record CreateAdmissionDto
    {
        [Required]
        public int PatientId { get; init; }

        [Required]
        public int BedId { get; init; }

        [Required]
        public int AdmittingDoctorId { get; init; }

        [Required]
        public DateTime AdmissionDate { get; init; }

        public DateOnly? ExpectedDischargeDate { get; init; }

        [Required, MaxLength(500)]
        public string AdmissionReason { get; init; } = string.Empty;

        [Required, MaxLength(100)]
        public string AdmittedBy { get; init; }=string.Empty;
    }
}
