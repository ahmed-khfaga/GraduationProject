using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
    public class ExerciseDetailDTOs : ExerciseSummaryDTOs
    {
        public string? Description { get; set; }
        public string? SecondaryMuscles { get; set; }
        public string? EquipmentNeeded { get; set; }
        public string? Instructions { get; set; }
        public string? CommonMistakes { get; set; }
    }
}
