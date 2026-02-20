using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs
{
    public class ExerciseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PrimaryMuscles { get; set; }
        public string? VideoUrl { get; set; }
        public string FitnessLevel { get; set; }
    }

    public class ExerciseDetailDto : ExerciseSummaryDto
    {
        public string? Description { get; set; }
        public string? SecondaryMuscles { get; set; }
        public string? EquipmentNeeded { get; set; }
        public string? Instructions { get; set; }
        public string? CommonMistakes { get; set; }
    }

    public class CreateExerciseDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? PrimaryMuscles { get; set; }
        public string? SecondaryMuscles { get; set; }
        public string? EquipmentNeeded { get; set; }
        public FitnessLevel FitnessLevel { get; set; }
        public string? VideoUrl { get; set; }
        public string? Instructions { get; set; }
        public string? CommonMistakes { get; set; }
    }

    // Exercise entry inside a session
    public class SessionExerciseDto
    {
        public int ExerciseID { get; set; }
        public string ExerciseName { get; set; }
        public string SectionType { get; set; }
        //public int OrderInSection { get; set; }
        public int? Sets { get; set; }
        public string? Reps { get; set; }
        public int? RestSeconds { get; set; }
        public string? Tempo { get; set; }
        public int? RPETarget { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }
    }

    public class WorkoutSessionDto
    {
        public int Id { get; set; }
        public string SessionTitle { get; set; }
        public string WeekDay { get; set; }
        public int EstimatedDuration { get; set; }
        public string? WarmupNotes { get; set; }
        public string? PrimerNotes { get; set; }
        public string? CooldownNotes { get; set; }
        public List<SessionExerciseDto> Exercises { get; set; } = new();
    }

    // Coach builds sessions when creating a program week
    public class CreateSessionExerciseDto
    {
        public int ExerciseID { get; set; }
        public SectionType SectionType { get; set; }
        public int OrderInSection { get; set; } = 1;
        public int? Sets { get; set; }
        public string? Reps { get; set; }
        public int? RestSeconds { get; set; }
        public string? Tempo { get; set; }
        public int? RPETarget { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateWorkoutSessionDto
    {
        public string SessionTitle { get; set; }
        public WeekDay WeekDay { get; set; }
        public int EstimatedDuration { get; set; }
        public string? WarmupNotes { get; set; }
        public string? PrimerNotes { get; set; }
        public string? CooldownNotes { get; set; }
        public List<CreateSessionExerciseDto> Exercises { get; set; } = new();
    }

    public class CreateProgramWeekDto
    {
        public int WeekNumber { get; set; }
        public string? WeekDescription { get; set; }
        public string? FocusArea { get; set; }
        public List<CreateWorkoutSessionDto> Sessions { get; set; } = new();
    }
}
