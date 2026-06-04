using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos
{
    public record CreateQualificationDto
    {
        [Required,MaxLength(100)]
        public string Degree { get; init; } = string.Empty;

        [Required,MaxLength(200)]
        public string Institution { get; init; } =string.Empty;

        [Required,Range(1950,2100)]
        public int YearAwarded { get; init; }

        [Required,MaxLength(100)]
        public string Country { get; init; } = string.Empty;
    }
}
