using AutoMapper;
using HMS.DAL.Models.DoctorModule;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.DoctorModule
{
    public class ScheduleProfile :Profile
    {
        public ScheduleProfile()
        {
            CreateMap<CreateScheduleDto, DoctorSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore());

            CreateMap<DoctorSchedule, ScheduleResultDto>()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek.ToString()));

        }
    }
}
