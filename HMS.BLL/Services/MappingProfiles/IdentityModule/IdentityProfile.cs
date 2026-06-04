using AutoMapper;
using HMS.DAL.Models.IdentityModule;
using HMS.BLL.Shared.Dtos.UserManagementDtos;

namespace HMS.BLL.Services.MappingProfiles.IdentityModule
{
    public class IdentityProfile : Profile
    {
        public IdentityProfile()
        {
            CreateMap<ApplicationUser, UserInfoDto>()
    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));
        }
    }
}
