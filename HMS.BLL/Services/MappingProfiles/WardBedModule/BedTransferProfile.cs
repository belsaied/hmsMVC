using AutoMapper;
using HMS.DAL.Models.WardBedModule;
using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.WardBedModule
{
    public class BedTransferProfile :Profile
    {
        public BedTransferProfile()
        {
            CreateMap<BedTransfer, BedTransferResultDto>()
               .ForMember(d => d.FromBedNumber,
                   o => o.MapFrom(s => s.FromBed != null ? s.FromBed.BedNumber : string.Empty))
               .ForMember(d => d.ToBedNumber,
                   o => o.MapFrom(s => s.ToBed != null ? s.ToBed.BedNumber : string.Empty));
        }
    }
}
