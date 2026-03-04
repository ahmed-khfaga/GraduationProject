using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.ProfileDTOs;
using FitZone.Service.DTOs.ProgramDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;

namespace FitZone.APIs.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── Track ────────────────────────────────────────────────
            CreateMap<Track, TrackDto>();

            // ── WorkoutProgram ───────────────────────────────────────
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

            // ── Exercise ─────────────────────────────────────────────
            CreateMap<Exercise, ExerciseSummaryDto>()
                .ForMember(d => d.FitnessLevel, o => o.MapFrom(s => s.FitnessLevel.ToString()));

            CreateMap<Exercise, ExerciseDetailDto>()
                .IncludeBase<Exercise, ExerciseSummaryDto>();

            CreateMap<CreateExerciseDto, Exercise>();
            CreateMap<CreateExerciseDto, Exercise>()
                .ReverseMap();          // for update: map dto onto existing entity

            // ── SessionExercise ──────────────────────────────────────
            CreateMap<SessionExercise, SessionExerciseDto>()
                .ForMember(d => d.ExerciseName, o => o.MapFrom(s => s.Exercise.Name))
                .ForMember(d => d.VideoUrl, o => o.MapFrom(s => s.Exercise.VideoUrl))
                .ForMember(d => d.SectionType, o => o.MapFrom(s => s.SectionType.ToString()));

            // ── WorkoutSession ───────────────────────────────────────
            CreateMap<WorkoutSession, WorkoutSessionDto>()
                .ForMember(d => d.WeekDay, o => o.MapFrom(s => s.weekDay.ToString()));

            // ── Enrollment ───────────────────────────────────────────
            CreateMap<TraineeProgramEnrollment, EnrollmentDto>()
                .ForMember(d => d.ProgramName, o => o.MapFrom(s => s.WorkoutProgram.Name))
                .ForMember(d => d.TrackName, o => o.MapFrom(s => s.Track.Name))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ── Coach profile ────────────────────────────────────────
            CreateMap<Coach, CoachProfileDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.ApplicationUser.FullName))
                .ForMember(d => d.ProgramCount, o => o.Ignore());   // set manually

            // ── Trainee profile ──────────────────────────────────────
            CreateMap<Trainee, TraineeProfileDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.ApplicationUser.FullName))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.ApplicationUser.Email));
        }
    }
}
