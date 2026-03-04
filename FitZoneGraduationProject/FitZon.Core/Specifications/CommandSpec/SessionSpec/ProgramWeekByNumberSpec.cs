using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.SessionSpec
{
    // Get a ProgramWeek by its program + week number (used to serve the trainee's current week)
    public class ProgramWeekByNumberSpec : BaseSpecatifications<ProgramWeek>
    {
        public ProgramWeekByNumberSpec(int programId, int weekNumber) : base(w =>
            w.WorkoutProgramID == programId &&
            w.WeekNumber == weekNumber)
        {
            Includes.Add(w => w.WorkoutSessions);
        }
    }
}
