using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    public class SessionExercise : BaseEntity
    {
        public int WorkoutSessionID { get; set; }

        public int ExerciseID { get; set; }

        public SectionType SectionType { get; set; }

        // Controls display order within a section
        public int OrderInSection { get; set; } = 1;
        public int? Sets { get; set; }

        public string? Reps { get; set; }  // "12", "8-10", "AMRAP"

        public int? RestSeconds { get; set; }


        public string? Tempo { get; set; }

        public int? RPETarget { get; set; }

        public string? Notes { get; set; }

        public virtual WorkoutSession WorkoutSession { get; set; }

        public virtual Exercise Exercise { get; set; }

    }
}
