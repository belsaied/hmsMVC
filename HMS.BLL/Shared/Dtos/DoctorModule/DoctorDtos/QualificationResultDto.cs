using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos
{
    public record QualificationResultDto
    {
        public int Id { get; init; }
        public int DoctorId { get; init; }
        public string Degree { get; init; } = string.Empty;
        public string Institution { get; init; } = string.Empty;
        public int YearAwarded { get; init; }
        public string Country { get; init; }= string.Empty;
    }
}
