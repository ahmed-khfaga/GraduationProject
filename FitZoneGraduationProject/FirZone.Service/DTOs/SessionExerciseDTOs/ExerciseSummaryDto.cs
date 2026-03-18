using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
    public class ExerciseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PrimaryMuscles { get; set; }
        public string? VideoUrl { get; set; }
        public string FitnessLevel { get; set; }
        public bool IsGlobal { get; set; }
    }
}
