using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ExerciseSpec
{
    // Returns ALL exercises a coach is allowed to use
    public class ExercisesForCoachSpec : BaseSpecatifications<Exercise>
    {
        public ExercisesForCoachSpec(int coachId, ExerciseFilterParams p) : base(e =>
            (e.CoachId == null || e.CoachId == coachId) &&
            (!p.Level.HasValue || e.FitnessLevel == p.Level) &&
            (string.IsNullOrWhiteSpace(p.Muscle) || (e.PrimaryMuscles != null && e.PrimaryMuscles.Contains(p.Muscle))) &&
            (string.IsNullOrWhiteSpace(p.Equipment) || (e.EquipmentNeeded != null && e.EquipmentNeeded.Contains(p.Equipment))))
        {
            OrderBy = e => e.Name;
            ApplyPagination(p.PageIndex, p.PageSize);
        }

        // Count overload (no pagination)
        public ExercisesForCoachSpec(int coachId, ExerciseFilterParams p, bool countOnly) : base(e =>
            (e.CoachId == null || e.CoachId == coachId) &&
            (!p.Level.HasValue || e.FitnessLevel == p.Level) &&
            (string.IsNullOrWhiteSpace(p.Muscle) || (e.PrimaryMuscles != null && e.PrimaryMuscles.Contains(p.Muscle))) &&
            (string.IsNullOrWhiteSpace(p.Equipment) || (e.EquipmentNeeded != null && e.EquipmentNeeded.Contains(p.Equipment))))
        {
        }
    }
}
