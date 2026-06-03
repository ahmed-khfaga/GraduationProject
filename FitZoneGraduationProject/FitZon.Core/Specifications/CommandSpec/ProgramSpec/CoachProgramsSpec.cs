using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{
    // All programs belonging to a specific coach
    public class CoachProgramsSpec : BaseSpecatifications<WorkoutProgram>
    {
        public CoachProgramsSpec(int coachId) : base(w => w.CoachId == coachId)
        {
            Includes.Add(w => w.Track);
            Includes.Add(w => w.Coach);
            IncludeStrings.Add("Coach.ApplicationUser");
            OrderByDescending = w => w.Id;
        }
    }
}
