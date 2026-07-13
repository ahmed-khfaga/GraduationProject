using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{
    public class ProgramWeekByIdAndCoachSpec : BaseSpecatifications<ProgramWeek>
    {
        public ProgramWeekByIdAndCoachSpec(int weekId, int coachId)
            : base(w => w.Id == weekId && w.WorkoutProgram.CoachId == coachId)
        {
            // Include WorkoutProgram so the navigation property is available
            // after the query (e.g. for logging or mapping in future use).
            Includes.Add(w => w.WorkoutProgram);
        }
    }
}

