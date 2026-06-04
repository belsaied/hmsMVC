using AutoMapper;
using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.WardBedModule;
using HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos;
using HMS.BLL.Shared.Dtos.WardBedModule.WardDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.WardBedModule
{
    public class WardProfile :Profile
    {
        public WardProfile()
        {
            CreateMap<CreateWardDto, Ward>()
                 .ForMember(d => d.Id, o => o.Ignore())
                 .ForMember(d => d.IsActive, o => o.Ignore())
                 .ForMember(d => d.Rooms, o => o.Ignore());

            CreateMap<Ward, WardResultDto>()
                .ForMember(d => d.WardType, o => o.MapFrom(s => s.WardType.ToString()));

            CreateMap<Ward, WardOccupancySummaryDto>()
                .ForMember(d => d.WardId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.WardType, o => o.MapFrom(s => s.WardType.ToString()))
                .ForMember(d => d.TotalBeds, o => o.Ignore())
                .ForMember(d => d.OccupiedBeds, o => o.Ignore())
                .ForMember(d => d.AvailableBeds, o => o.Ignore())
                .ForMember(d => d.MaintenanceBeds, o => o.Ignore())
                .ForMember(d => d.ReservedBeds, o => o.Ignore());

            CreateMap<Ward, WardWithDetailsResultDto>()
                .ForMember(d => d.WardType, o => o.MapFrom(s => s.WardType.ToString()))
                .ForMember(d => d.Rooms, o => o.MapFrom(s => s.Rooms));

        }
    }
}
