using AutoMapper;
using HMS.DAL.Models.WardBedModule;
using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.MappingProfiles.WardBedModule
{
    public class AdmissionProfile :Profile
    {
        public AdmissionProfile()
        {
            CreateMap<CreateAdmissionDto, Admission>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.ActualDischargeDate, o => o.Ignore())
                .ForMember(d => d.DischargeSummary, o => o.Ignore())
                .ForMember(d => d.Patient, o => o.Ignore())
                .ForMember(d => d.Bed, o => o.Ignore())
                .ForMember(d => d.AdmittingDoctor, o => o.Ignore())
                .ForMember(d => d.Transfers, o => o.Ignore());

            CreateMap<Admission, AdmissionResultDto>()
                .ForMember(d => d.PatientName,
                    o => o.MapFrom(s => s.Patient != null
                        ? $"{s.Patient.FirstName} {s.Patient.LastName}"
                        : string.Empty))
                .ForMember(d => d.BedNumber,
                    o => o.MapFrom(s => s.Bed != null ? s.Bed.BedNumber : string.Empty))
                .ForMember(d => d.RoomNumber,
                    o => o.MapFrom(s => s.Bed != null && s.Bed.Room != null
                        ? s.Bed.Room.RoomNumber
                        : string.Empty))
                .ForMember(d => d.WardName,
                    o => o.MapFrom(s => s.Bed != null && s.Bed.Room != null && s.Bed.Room.Ward != null
                        ? s.Bed.Room.Ward.Name
                        : string.Empty))
                .ForMember(d => d.DoctorName,
                    o => o.MapFrom(s => s.AdmittingDoctor != null
                        ? $"{s.AdmittingDoctor.FirstName} {s.AdmittingDoctor.LastName}"
                        : string.Empty))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Transfers,
                    o => o.MapFrom(s => s.Transfers));
        }
    }
}
