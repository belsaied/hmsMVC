using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.WardDtos
{
    public record WardOccupancySummaryDto
    {
        public int WardId { get; init; }
        public string WardName { get; init; } = string.Empty;
        public string WardType { get; init; } = string.Empty;
        public int TotalBeds { get; init; }
        public int? Floor { get; init; }
        public bool IsActive { get; init; }
        public int OccupiedBeds { get; init; }
        public int AvailableBeds { get; init; }
        public int MaintenanceBeds { get; init; }
        public int ReservedBeds { get; init; }
        //public double OccupancyPercentage =>TotalBeds == 0 ? 0 : Math.Round((double)OccupiedBeds / TotalBeds * 100, 1);
    }
}
