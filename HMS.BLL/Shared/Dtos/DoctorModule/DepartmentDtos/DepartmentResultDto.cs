using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos
{
    public record DepartmentResultDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? PhoneExtension { get; init; }
        public int? HeadDoctorId { get; init; }
        public string? HeadDoctorName { get; init; }
    }
}
