using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class AvailableBedSpecification :BaseSpecifications<Bed,int>
    {
        public AvailableBedSpecification(WardType? wardType, BedType? bedType)
            : base(b =>
                b.Status == BedStatus.Available &&
                b.Room.IsActive &&
                b.Room.Ward.IsActive &&
                (!wardType.HasValue || b.Room.Ward.WardType == wardType.Value) &&
                (!bedType.HasValue || b.BedType == bedType.Value))
        {
            AddInclude(b => b.Room);
            // Room.Ward loaded via ThenInclude in Infrastructure
            AddOrderBy(b => b.BedNumber);
        }

    }
}
