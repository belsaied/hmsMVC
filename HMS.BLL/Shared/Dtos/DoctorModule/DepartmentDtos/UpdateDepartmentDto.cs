using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos
{
    public record UpdateDepartmentDto
    {
        [MaxLength(100)]
        public string? Name { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [MaxLength(20)]
        public string? PhoneExtension { get; init; }

        public int? HeadDoctorId { get; init; }  
    }
}
