using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProgramDTOs
{

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

    
}
