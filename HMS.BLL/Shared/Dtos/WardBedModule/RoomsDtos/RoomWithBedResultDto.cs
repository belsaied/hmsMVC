using HMS.BLL.Shared.Dtos.WardBedModule.BedDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos
{
    public record RoomWithBedResultDto
    {
        public int Id { get; init; }
        public string RoomNumber { get; init; } = string.Empty;
        public string RoomType { get; init; } = string.Empty;
        public int Capacity { get; init; }
        public bool IsActive { get; init; }
        public string? Notes { get; init; }
        public IEnumerable<BedResultDto> Beds { get; init; } = [];
    }
}
