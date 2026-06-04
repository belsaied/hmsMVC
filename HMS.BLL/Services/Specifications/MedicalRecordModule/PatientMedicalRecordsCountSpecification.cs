using HMS.DAL.Models.MedicalRecordModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.MedicalRecordModule
{
    public class PatientMedicalRecordsCountSpecification : BaseSpecifications<MedicalRecord,int>
    {
        public PatientMedicalRecordsCountSpecification(int patientId) 
            : base(r=>r.PatientId == patientId)
        {
            
        }
    }
}
