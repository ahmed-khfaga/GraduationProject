using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.ProfileDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingTrainee : Profile
    {
        public MappingTrainee() 
        {
            CreateMap<Trainee, TraineeProfileDto>()
                   .ForMember(d => d.FullName, o => o.MapFrom(s => s.ApplicationUser.FullName))
                   .ForMember(d => d.Email, o => o.MapFrom(s => s.ApplicationUser.Email));
        }
    }
}
