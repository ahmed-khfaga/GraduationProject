using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{

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

  
}
