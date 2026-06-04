using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class AllWardsWithRoomsSpecification :BaseSpecifications<Ward,int>
    {
        public AllWardsWithRoomsSpecification() : base()
        {
            AddInclude(w => w.Rooms);

            // FIX: WardService.GetAllWardsWithOccupancyAsync() does:
            //   w.Rooms.SelectMany(r => r.Beds)
            // Without this include, Beds collection is empty → all counts = 0
            AddInclude("Rooms.Beds");

            AddOrderBy(w => w.Name);
        }

    }
}
