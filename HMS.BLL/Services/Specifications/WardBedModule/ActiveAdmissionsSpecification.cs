using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class ActiveAdmissionsSpecification :BaseSpecifications<Admission,int>
    {
        public ActiveAdmissionsSpecification() : base(a => a.Status == AdmissionStatus.Active)
        {
            AddInclude(a => a.Patient);
            AddInclude(a => a.AdmittingDoctor);

            // FIX: Same as AdmissionWithDetailsSpecification
            // AdmissionResultDto needs BedNumber, RoomNumber, WardName
            AddInclude(a => a.Bed);
            AddInclude("Bed.Room");
            AddInclude("Bed.Room.Ward");

            AddOrderByDescending(a => a.AdmissionDate);
        }

    }
}
