using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.SessionExerciseDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingExercise : Profile
    {
        public MappingExercise() 
        {
            CreateMap<Exercise, ExerciseSummaryDto>()
                    .ForMember(d => d.FitnessLevel, o => o.MapFrom(s => s.FitnessLevel.ToString()));

            CreateMap<Exercise, ExerciseDetailDto>()
                .IncludeBase<Exercise, ExerciseSummaryDto>();

            CreateMap<CreateExerciseDto, Exercise>();
            CreateMap<CreateExerciseDto, Exercise>().ReverseMap();
        }
    }
}
