using AutoMapper;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.Services.MappingProfiles.MedicalRecordModule
{
    public class LabResultProfile : Profile
    {
        public LabResultProfile()
        {
            CreateMap<CreateLabResultDto, LabResult>()
    .ForMember(d => d.Id, o => o.Ignore())
    .ForMember(d => d.LabOrderId, o => o.Ignore())
    .ForMember(d => d.LabOrder, o => o.Ignore());

            CreateMap<LabResult, LabResultResultDto>();
        }
    }
}
