using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.ProgramDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingWorkoutProgram : Profile
    {
        public MappingWorkoutProgram() 
        {
            CreateMap<WorkoutProgram, ProgramCardDto>()
                    .ForMember(d => d.TrackName, o => o.MapFrom(s => s.Track.Name))
                    .ForMember(d => d.CoachName, o => o.MapFrom(s => s.Coach.ApplicationUser.FullName))
                    .ForMember(d => d.CoachRating, o => o.MapFrom(s => s.Coach.Rating))
                    .ForMember(d => d.TrainingGoal, o => o.MapFrom(s => s.TrainingGoal.ToString()))
                    .ForMember(d => d.FitnessLevel, o => o.MapFrom(s => s.FitnessLevel.ToString()))
                    .ForMember(d => d.EquipmentType, o => o.MapFrom(s => s.EquipmentType.ToString()));

            CreateMap<WorkoutProgram, ProgramDetailDto>()
                .IncludeBase<WorkoutProgram, ProgramCardDto>()
                .ForMember(d => d.ProgramWeekSummaryDto, o => o.MapFrom(s => s.ProgramWeeks));

            CreateMap<ProgramWeek, ProgramWeekSummaryDto>()
                .ForMember(d => d.SessionCount, o => o.MapFrom(s => s.WorkoutSessions.Count));

            CreateMap<CreateProgramDto, WorkoutProgram>();

        }
    }
}
