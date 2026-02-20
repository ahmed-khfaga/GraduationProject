using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs
{
    // Shown on the catalogue card
    public class ProgramCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TrackName { get; set; }
        public string CoachName { get; set; }
        public decimal? CoachRating { get; set; }
        public int DurationOnWeeks { get; set; }
        public int SessionsPerWeeks { get; set; }
        public int SessionsDuration { get; set; }
        public string TrainingGoal { get; set; }
        public string FitnessLevel { get; set; }
        public string EquipmentType { get; set; }
        public string? PhotoThumbnailUrl { get; set; }
    }

    // Week + session summary inside program detail
    public class ProgramWeekSummaryDto
    {
        public int WeekNumber { get; set; }
        public string? WeekDescription { get; set; }
        public string? FocusArea { get; set; }
        public int SessionCount { get; set; }
    }

    // Full program detail page
    public class ProgramDetailDto : ProgramCardDto
    {
        public List<ProgramWeekSummaryDto> Weeks { get; set; } = new();
    }

    // Coach submits or edits a program
    public class CreateProgramDto
    {
        public int TrackID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationOnWeeks { get; set; }
        public int SessionsPerWeeks { get; set; }
        public int SessionsDuration { get; set; }
        public TrainingGoal TrainingGoal { get; set; }
        public FitnessLevel FitnessLevel { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public string? PhotoThumbnailUrl { get; set; }
    }

    public class UpdateProgramDto : CreateProgramDto { }
}
