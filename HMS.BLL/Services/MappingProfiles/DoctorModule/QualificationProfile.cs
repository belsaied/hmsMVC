using AutoMapper;
using HMS.DAL.Models.DoctorModule;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.DoctorModule
{
    public class QualificationProfile :Profile
    {
        public QualificationProfile()
        {
            CreateMap<CreateQualificationDto, DoctorQualification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore());


            CreateMap<DoctorQualification, QualificationResultDto>();
        }
    }
}
