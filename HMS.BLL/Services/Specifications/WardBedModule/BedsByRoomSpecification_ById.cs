using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class BedsByRoomSpecification_ById : BaseSpecifications<Bed, int>
    {
        public BedsByRoomSpecification_ById(int bedId) : base(b => b.Id == bedId)
        {
            AddInclude(b => b.Room);
            AddInclude("Room.Ward");
        }
    }

}
