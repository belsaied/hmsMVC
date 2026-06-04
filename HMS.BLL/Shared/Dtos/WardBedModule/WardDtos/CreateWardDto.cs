using HMS.DAL.Models.Enums.WardBedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.WardDtos
{
    public record CreateWardDto
    {
        [Required, MaxLength(100)]
        public string Name { get; init; } = string.Empty;

        [Required]
        public WardType WardType { get; init; }

        public int? Floor { get; init; }

        [MaxLength(20)]
        public string? PhoneExtension { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }
    }
}
