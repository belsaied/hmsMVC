using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos
{
    public record TransferBedDto
    {
        [Required]
        public int ToBedId { get; init; }

        [Required, MaxLength(500)]
        public string Reason { get; init; } = string.Empty;
       
        [Required, MaxLength(100)]
        public string TransferredBy { get; init; } = string.Empty;
    }
}
