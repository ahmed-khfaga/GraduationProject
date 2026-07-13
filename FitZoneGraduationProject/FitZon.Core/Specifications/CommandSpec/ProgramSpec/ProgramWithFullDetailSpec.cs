using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{
    // Full program detail with all nested data
    public class ProgramWithFullDetailSpec : BaseSpecatifications<WorkoutProgram>
    {
        public ProgramWithFullDetailSpec(int programId) : base(w => w.Id == programId)
        {
            Includes.Add(w => w.Track);
            Includes.Add(w => w.Coach);
            IncludeStrings.Add("Coach.ApplicationUser");
            Includes.Add(w => w.ProgramWeeks);

            IncludeStrings.Add("ProgramWeeks.WorkoutSessions");
        }
    }
}
