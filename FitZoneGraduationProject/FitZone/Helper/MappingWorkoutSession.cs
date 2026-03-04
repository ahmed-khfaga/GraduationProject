using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingWorkoutSession : Profile
    {
        public MappingWorkoutSession() 
        {
            // ── WorkoutSession ───────────────────────────────────────
            CreateMap<WorkoutSession, WorkoutSessionDto>()
                .ForMember(d => d.WeekDay, o => o.MapFrom(s => s.weekDay.ToString()))
                .ForMember(d => d.SessionExerciseDto, o => o.Ignore()); // populated manually in service

            CreateMap<WorkoutSession, SessionSummaryDto>()
                .ForMember(d => d.WeekDay, o => o.MapFrom(s => s.weekDay.ToString()));
        }
    }
}
