using AutoMapper;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.Services.MappingProfiles.MedicalRecordModule
{
    public class LabOrderProfile : Profile
    {
        public LabOrderProfile()
        {
            CreateMap<CreateLabOrderDto, LabOrder>()
    .ForMember(d => d.Id, o => o.Ignore())
    .ForMember(d => d.MedicalRecordId, o => o.Ignore())
    .ForMember(d => d.PatientId, o => o.Ignore())
    .ForMember(d => d.DoctorId, o => o.Ignore())
    .ForMember(d => d.OrderedAt, o => o.Ignore())
    .ForMember(d => d.Status, o => o.Ignore())
    .ForMember(d => d.MedicalRecord, o => o.Ignore())
    .ForMember(d => d.Patient, o => o.Ignore())
    .ForMember(d => d.Doctor, o => o.Ignore())
    .ForMember(d => d.Result, o => o.Ignore());

            CreateMap<LabOrder, LabOrderResultDto>()
                .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}
