using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos
{
    public record RoomResultDto
    {
        public int Id { get; init; }
        public int WardId { get; init; }
        public string WardName { get; init; } = string.Empty;
        public string RoomNumber { get; init; } = string.Empty;
        public string RoomType { get; init; } = string.Empty;
        public int Capacity { get; init; }
        public bool IsActive { get; init; }
        public string? Notes { get; init; }
    }
}
