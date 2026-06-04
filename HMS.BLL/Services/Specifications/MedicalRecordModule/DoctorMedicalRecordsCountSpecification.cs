using HMS.DAL.Models.MedicalRecordModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class DoctorMedicalRecordsCountSpecification :BaseSpecifications<MedicalRecord,int>
    {
        public DoctorMedicalRecordsCountSpecification(int doctorId) 
            : base(r=>r.DoctorId ==doctorId)
        {
            
        }
    }
}
