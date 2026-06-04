using AutoMapper;
using HMS.DAL.Models.WardBedModule;
using HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.WardBedModule
{
    public class RoomProfile :Profile
    {
        public RoomProfile()
        {
            CreateMap<CreateRoomDto, Room>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.WardId, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.Ward, o => o.Ignore())
                .ForMember(d => d.Beds, o => o.Ignore());

            CreateMap<Room, RoomResultDto>()
                .ForMember(d => d.WardName, o => o.MapFrom(s => s.Ward != null ? s.Ward.Name : string.Empty))
                .ForMember(d => d.RoomType, o => o.MapFrom(s => s.RoomType.ToString()));

            // Room → RoomWithBedResultDto (used inside WardWithDetailsResultDto)
            CreateMap<Room, RoomWithBedResultDto>()
                .ForMember(d => d.RoomType, o => o.MapFrom(s => s.RoomType.ToString()))
                .ForMember(d => d.Beds, o => o.MapFrom(s => s.Beds));

        }
    }
}
