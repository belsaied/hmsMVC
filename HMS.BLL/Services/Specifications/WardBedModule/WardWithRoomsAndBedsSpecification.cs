using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class WardWithRoomsAndBedsSpecification :BaseSpecifications<Ward,int>
    {
        public WardWithRoomsAndBedsSpecification(int wardId) : base(w => w.Id == wardId)
        {
            AddInclude(w => w.Rooms);

            //  FIX: Was "Room.Ward" (singular, wrong navigation name)
            // Correct deep includes for WardWithDetailsResultDto
            // which needs: Ward → Rooms → Beds
            AddInclude("Rooms.Beds");
        }

    }
}
