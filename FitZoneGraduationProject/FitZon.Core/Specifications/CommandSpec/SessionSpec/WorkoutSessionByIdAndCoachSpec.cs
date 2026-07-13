using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.SessionSpec
{
    public class WorkoutSessionByIdAndCoachSpec : BaseSpecatifications<WorkoutSession>
    {
        public WorkoutSessionByIdAndCoachSpec(int sessionId, int coachId)
            : base(s => s.Id == sessionId
                     && s.ProgramWeek.WorkoutProgram.CoachId == coachId)
        {
            // Level 1: Session → ProgramWeek
            Includes.Add(s => s.ProgramWeek);

            // Level 2: ProgramWeek → WorkoutProgram (nested — use string form)
            IncludeStrings.Add("ProgramWeek.WorkoutProgram");
        }
    }
}
