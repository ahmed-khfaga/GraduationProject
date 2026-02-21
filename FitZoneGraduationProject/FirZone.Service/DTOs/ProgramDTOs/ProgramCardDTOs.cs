using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProgramDTOs
{
    // Shown on the catalogue card
    public class ProgramCardDTOs
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
}
