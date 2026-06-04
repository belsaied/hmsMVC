using HMS.DAL.Models.Enums.DoctorEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos
{
    public record UpdateDoctorDto
    {
        [MinLength(2), MaxLength(50),
         RegularExpression(@"^[\p{L}\s'\-]+$",
             ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string? FirstName { get; init; }

        [MinLength(2), MaxLength(50),
         RegularExpression(@"^[\p{L}\s'\-]+$",
             ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string? LastName { get; init; }

        [Phone]
        public string? Phone { get; init; }

        [EmailAddress]
        public string? Email { get; init; }

        [MaxLength(100)]
        public string? Specialization { get; init; }

        public int? DepartmentId { get; init; }

        [Range(0, 60)]
        public int? YearsOfExperience { get; init; }

        [Range(0, 100000)]
        public decimal? ConsultationFee { get; init; }

        [MaxLength(1000)]
        public string? Bio { get; init; }

        public string? PictureUrl { get; init; }

        public DoctorStatus? Status { get; init; }
    }
}
