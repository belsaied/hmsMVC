using HMS.DAL.Models.DoctorModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class DoctorQualificationSpecification :BaseSpecifications<DoctorQualification,int>
    {
        //لجيب كل مؤهلات دكتور معين
        public DoctorQualificationSpecification(int doctorId)
           : base(q => q.DoctorId == doctorId)
        {
        }
    }
}
