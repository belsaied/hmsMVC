using AutoMapper;
using HMS.DAL.Models.WardBedModule;
using HMS.BLL.Shared.Dtos.WardBedModule.BedDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.WardBedModule
{
    public class BedProfile :Profile
    {
        public BedProfile()
        {
            CreateMap<CreateBedDto, Bed>()
               .ForMember(d => d.Id, o => o.Ignore())
               .ForMember(d => d.RoomId, o => o.Ignore())
               .ForMember(d => d.Status, o => o.Ignore())
               .ForMember(d => d.Room, o => o.Ignore())
               .ForMember(d => d.Admissions, o => o.Ignore());

            CreateMap<Bed, BedResultDto>()
                .ForMember(d => d.RoomNumber, o => o.MapFrom(s => s.Room != null ? s.Room.RoomNumber : string.Empty))
                .ForMember(d => d.WardName, o => o.MapFrom(s => s.Room != null && s.Room.Ward != null ? s.Room.Ward.Name : string.Empty))
                .ForMember(d => d.BedType, o => o.MapFrom(s => s.BedType.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // Bed → BedAvailabilityResultDto
            CreateMap<Bed, BedAvailabilityResultDto>()
                .ForMember(d => d.BedId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.BedType, o => o.MapFrom(s => s.BedType.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.RoomId, o => o.MapFrom(s => s.Room != null ? s.Room.Id : 0))
                .ForMember(d => d.RoomNumber, o => o.MapFrom(s => s.Room != null ? s.Room.RoomNumber : string.Empty))
                .ForMember(d => d.RoomType, o => o.MapFrom(s => s.Room != null ? s.Room.RoomType.ToString() : string.Empty))
                .ForMember(d => d.WardId, o => o.MapFrom(s => s.Room != null ? s.Room.WardId : 0))
                .ForMember(d => d.WardName, o => o.MapFrom(s => s.Room != null && s.Room.Ward != null ? s.Room.Ward.Name : string.Empty))
                .ForMember(d => d.WardType, o => o.MapFrom(s => s.Room != null && s.Room.Ward != null ? s.Room.Ward.WardType.ToString() : string.Empty))
                .ForMember(d => d.Floor, o => o.MapFrom(s => s.Room != null && s.Room.Ward != null ? s.Room.Ward.Floor : null));
        }
    }
}
