using AutoMapper;
using HMS.DAL.Models.DoctorModule;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.DoctorModule
{
    public class DoctorProfile :Profile
    {
        public DoctorProfile()
        {
            //Mapping CreateDoctorDto -> Doctor
            CreateMap<CreateDoctorDto, Doctor>()
                .ForMember(deset => deset.Id, opt => opt.Ignore())
                .ForMember(deset => deset.JoinDate, opt => opt.Ignore())
                .ForMember(deset => deset.Status, opt => opt.Ignore())
                .ForMember(deset => deset.Department, opt => opt.Ignore())
                .ForMember(deset => deset.Qualifications, opt => opt.Ignore())
                .ForMember(deset => deset.AvailabilitySchedules, opt => opt.Ignore());
            
            // Doctor -> DoctorResultDto
            CreateMap<Doctor, DoctorResultDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<DoctorPictureUrlResolver<DoctorResultDto>>());

            // Doctor -> DoctorWithDetailsResultDto
            CreateMap<Doctor, DoctorWithDetailsResultDto>()
              .ForMember(dest => dest.Gender,
                  opt => opt.MapFrom(src => src.Gender.ToString()))
              .ForMember(dest => dest.Status,
                  opt => opt.MapFrom(src => src.Status.ToString()))
              .ForMember(dest => dest.DepartmentName,
                  opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
              .ForMember(dest => dest.Qualifications,
                  opt => opt.MapFrom(src => src.Qualifications))
              .ForMember(dest => dest.AvailabilitySchedules,
                  opt => opt.MapFrom(src => src.AvailabilitySchedules))
              .ForMember(dest => dest.PictureUrl,
                  opt => opt.MapFrom<DoctorPictureUrlResolver<DoctorWithDetailsResultDto>>());


        }
    }
}
