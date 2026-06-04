using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class AdmissionWithDetailsSpecification :BaseSpecifications<Admission,int>
    {
        public AdmissionWithDetailsSpecification(int id) : base(a => a.Id == id)
        {
            AddInclude(a => a.Patient);
            AddInclude(a => a.AdmittingDoctor);

            //  FIX: Must deep-load Bed → Room → Ward
            // AdmissionResultDto needs: BedNumber, RoomNumber, WardName
            // These come from: a.Bed.BedNumber, a.Bed.Room.RoomNumber, a.Bed.Room.Ward.Name
            AddInclude(a => a.Bed);
            AddInclude("Bed.Room");
            AddInclude("Bed.Room.Ward");

            //  Also include Transfers for BedTransferResultDto
            AddInclude(a => a.Transfers);
        }

    }
}
