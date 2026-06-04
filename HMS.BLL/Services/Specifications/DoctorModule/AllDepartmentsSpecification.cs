using HMS.DAL.Models.DoctorModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class AllDepartmentsSpecification :BaseSpecifications<Department,int>
    {
        // لجيب كل الأقسام مع اسم الدكتور المسؤول عن كل قسم
        public AllDepartmentsSpecification():base()
        {
            AddInclude(d => d.HeadDoctor!);
            AddOrderBy(d => d.Name);
        }
    }
}
