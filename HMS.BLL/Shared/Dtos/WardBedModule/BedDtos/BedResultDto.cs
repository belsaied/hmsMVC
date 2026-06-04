using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.BedDtos
{
    public record BedResultDto
    {
        public int Id { get; init; }
        public int RoomId { get; init; }
        public string RoomNumber { get; init; } = string.Empty;
        public string WardName { get; init; } = string.Empty;
        public string BedNumber { get; init; } = string.Empty;
        public string BedType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? Notes { get; init; }
    }
}
