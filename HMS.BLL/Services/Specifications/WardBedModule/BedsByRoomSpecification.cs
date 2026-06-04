using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class BedsByRoomSpecification :BaseSpecifications<Bed,int>
    {
        public BedsByRoomSpecification(int roomId) : base(b => b.RoomId == roomId)
        {
            //FIX: BedResultDto needs WardName → requires Room.Ward
            AddInclude(b => b.Room);
            AddInclude("Room.Ward");

            AddOrderBy(b => b.BedNumber);
        }

    }
}
