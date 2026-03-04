using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.SessionExerciseDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingSessionExercise : Profile
    {
        public MappingSessionExercise() 
        {
            // Note: ordering by section is done in service — AutoMapper just maps the flat list
            CreateMap<SessionExercise, SessionExerciseDto>()
                .ForMember(d => d.ExerciseName, o => o.MapFrom(s => s.Exercise.Name))
                .ForMember(d => d.VideoUrl, o => o.MapFrom(s => s.Exercise.VideoUrl))
                .ForMember(d => d.SectionType, o => o.MapFrom(s => s.SectionType.ToString()));

        }
    }
}
