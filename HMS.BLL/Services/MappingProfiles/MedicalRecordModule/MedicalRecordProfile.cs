using AutoMapper;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.Services.MappingProfiles.MedicalRecordModule
{
    public class MedicalRecordProfile : Profile
    {
        public MedicalRecordProfile()
        {
            // CreateMedicalRecordDto → MedicalRecord
            CreateMap<CreateMedicalRecordDto, MedicalRecord>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.RecordedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.Patient, o => o.Ignore())
                .ForMember(d => d.Doctor, o => o.Ignore())
                .ForMember(d => d.Appointment, o => o.Ignore())
                .ForMember(d => d.VitalSigns, o => o.Ignore())
                .ForMember(d => d.Prescriptions, o => o.Ignore())
                .ForMember(d => d.LabOrders, o => o.Ignore())
                // Map VisitDate → VisiteDate (entity typo kept as-is to not break DB)
                .ForMember(d => d.VisitDate, o => o.MapFrom(s => s.VisitDate))
                // Map Diagnosis → Diagnsis (entity typo kept as-is)
                .ForMember(d => d.Diagnosis, o => o.MapFrom(s => s.Diagnosis));

            // MedicalRecord → MedicalRecordResultDto
            CreateMap<MedicalRecord, MedicalRecordResultDto>()
                .ForMember(d => d.VisitDate, o => o.MapFrom(s => s.VisitDate))
                .ForMember(d => d.Diagnosis, o => o.MapFrom(s => s.Diagnosis))
                .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                    s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : string.Empty))
                .ForMember(d => d.DoctorName, o => o.MapFrom(s =>
                    s.Doctor != null ? $"{s.Doctor.FirstName} {s.Doctor.LastName}" : string.Empty));
        }
    }
}
