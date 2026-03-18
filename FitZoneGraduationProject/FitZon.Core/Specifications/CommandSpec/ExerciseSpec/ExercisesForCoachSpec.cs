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
            (e.CoachID == null || e.CoachID == coachId) &&
            (!p.Level.HasValue || e.FitnessLevel == p.Level))
        {
            OrderBy = e => e.Name;
            ApplyPagination(p.PageIndex, p.PageSize);
        }

        // Count overload (no pagination)
        public ExercisesForCoachSpec(int coachId, ExerciseFilterParams p, bool countOnly) : base(e =>
            (e.CoachID == null || e.CoachID == coachId) &&
            (!p.Level.HasValue || e.FitnessLevel == p.Level))
        {
        }
    }
}
