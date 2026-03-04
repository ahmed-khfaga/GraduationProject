using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.ProfileDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingCoach : Profile
    {
        public MappingCoach() 
        {
            CreateMap<Coach, CoachProfileDto>()
                  .ForMember(d => d.FullName, o => o.MapFrom(s => s.ApplicationUser.FullName))
                  .ForMember(d => d.ProgramCount, o => o.Ignore());
        }
    }
}
