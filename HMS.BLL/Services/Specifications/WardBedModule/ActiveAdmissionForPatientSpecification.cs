using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.WardBedModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public class ActiveAdmissionForPatientSpecification :BaseSpecifications<Admission,int>
    {
        public ActiveAdmissionForPatientSpecification(int patientId) : base(a => a.PatientId == patientId && a.Status == AdmissionStatus.Active)
        { 

        }

    }
}
