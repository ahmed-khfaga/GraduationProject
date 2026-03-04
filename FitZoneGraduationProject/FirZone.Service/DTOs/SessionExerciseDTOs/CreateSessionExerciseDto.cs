using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
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
}
