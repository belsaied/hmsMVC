using HMS.DAL.Models.DoctorModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class DepartmentWithHeadSpecification :BaseSpecifications<Department,int>
    {
        // لجيب قسم واحد مع اسم الدكتور المسؤول
        public DepartmentWithHeadSpecification(int id):base(d=>d.Id ==id)
        {
            AddInclude(d => d.HeadDoctor!);
        }
    }
}
