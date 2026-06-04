using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos
{
    public record DoctorWithDetailsResultDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public int Age { get; init; }
        public string Gender { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Specialization { get; init; } = string.Empty;
        public int DepartmentId { get; init; }
        public string DepartmentName { get; init; } = string.Empty;
        public int YearsOfExperience { get; init; }
        public decimal ConsultationFee { get; init; }
        public string? Bio { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? PictureUrl { get; init; }
        public DateTime JoinDate { get; init; }

        public IEnumerable<QualificationResultDto> Qualifications { get; init; } = [];
        public IEnumerable<ScheduleResultDto> AvailabilitySchedules { get; init; } = [];
    }
}
