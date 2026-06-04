using HMS.DAL.Models.Enums.WardBedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.BedDtos
{
    public record UpdateBedStatusDto
    {
        [Required]
        public BedStatus Status { get; init; }

        [MaxLength(500)]
        public string? Notes { get; init; }
    }
}
