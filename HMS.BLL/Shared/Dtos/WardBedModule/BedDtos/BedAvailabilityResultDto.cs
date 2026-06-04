using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.BedDtos
{
    public record BedAvailabilityResultDto
    {
        public int BedId { get; init; }
        public string BedNumber { get; init; } = string.Empty;
        public string BedType { get; init; } = string.Empty;
        public int RoomId { get; init; }           
        public string RoomNumber { get; init; } = string.Empty;
        public string RoomType { get; init; } = string.Empty; 
        public int WardId { get; init; }           
        public string WardName { get; init; } = string.Empty;
        public string WardType { get; init; } = string.Empty;
        public int? Floor { get; init; }
        public string Status { get; init; } = string.Empty;   
        public string? Notes { get; init; }
    }
}
