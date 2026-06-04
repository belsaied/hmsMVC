using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class PatientAdmissionHistorySpecification :BaseSpecifications<Admission,int>
    {
        public PatientAdmissionHistorySpecification(int patientId)
          : base(a => a.PatientId == patientId)
        {
            AddInclude(a => a.AdmittingDoctor);

            // FIX: Added deep include for Bed → Room → Ward
            // AdmissionResultDto needs BedNumber, RoomNumber, WardName
            AddInclude(a => a.Bed);
            AddInclude("Bed.Room");
            AddInclude("Bed.Room.Ward");

            AddOrderByDescending(a => a.AdmissionDate);
        }

    }
}
