using AutoMapper;
using HMS.DAL.Models.DoctorModule;
using HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.DepartmentModule
{
    public class DepartmentProfile :Profile
    {
        public DepartmentProfile()
        {
            // Mapping CreateDepartmentDto -> Department
            CreateMap<CreateDepartmentDto, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.HeadDoctorId, opt => opt.Ignore())
                .ForMember(dest => dest.HeadDoctor, opt => opt.Ignore())
                .ForMember(dest => dest.Doctors, opt => opt.Ignore());

            //Mapping Department -> DepartmentResultDto
            CreateMap<Department, DepartmentResultDto>()
                .ForMember(dest => dest.HeadDoctorName, opt => opt.MapFrom(src => src.HeadDoctor != null ? $"{src.HeadDoctor.FirstName} {src.HeadDoctor.LastName}" : null));
        }
    }
}
