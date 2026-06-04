using AutoMapper;
using HMS.DAL.Models.PatientModule;
using Microsoft.Extensions.Configuration;
using HMS.BLL.Shared.Dtos.PatientModule.PatientDtos;

namespace HMS.BLL.Services.MappingProfiles.PatientModule
{
    public class PatientPictureUrlResolver<TDestination>(IConfiguration _configuration)
        : IValueResolver<Patient, TDestination, string?>

    {
        public string? Resolve(Patient source, TDestination destination, string? destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.PictureUrl))
                return null;

            return $"{_configuration.GetSection("URLS")["BaseUrl"]}{source.PictureUrl}";
        }
    }
}
