using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ExerciseSpec
{
    //Fetch a single exercise — checks global OR owned by this coach
    public class ExerciseByIdForCoachSpec : BaseSpecatifications<Exercise>
    {
        public ExerciseByIdForCoachSpec(int exerciseId, int coachId) : base(e =>
            e.ID == exerciseId && (e.CoachID == null || e.CoachID == coachId))
        {
        }
    }
}
