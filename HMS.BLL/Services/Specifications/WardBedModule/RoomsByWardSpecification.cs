using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class RoomsByWardSpecification :BaseSpecifications<Room,int>
    {
        public RoomsByWardSpecification(int wardId) : base(r => r.WardId == wardId)
        {
            AddInclude(r => r.Ward);
            AddInclude(r => r.Beds);
            AddOrderBy(r => r.RoomNumber);
        }

    }
}
