using AutoMapper;
using HMS.DAL.Models.AppointmentModule;
using HMS.BLL.Shared.Dtos.AppointmentModule;

namespace HMS.BLL.Services.MappingProfiles.AppointmentModule
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<Appointment, AppointmentResultDto>()
    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.Type,
        opt => opt.MapFrom(src => src.Type.ToString()))
    .ForMember(dest => dest.PatientName,
        opt => opt.MapFrom(src =>
            src.Patient != null
            ? $"{src.Patient.FirstName} {src.Patient.LastName}"
            : string.Empty))
    .ForMember(dest => dest.DoctorName,
        opt => opt.MapFrom(src =>
            src.Doctor != null
            ? $"{src.Doctor.FirstName} {src.Doctor.LastName}"
            : string.Empty))
    .ForMember(dest => dest.DoctorSpecialization,
        opt => opt.MapFrom(src =>
            src.Doctor != null ? src.Doctor.Specialization : string.Empty));
        }
    }
}
