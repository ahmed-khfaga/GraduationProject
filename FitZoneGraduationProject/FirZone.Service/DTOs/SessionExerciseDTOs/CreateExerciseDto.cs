using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
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
}
