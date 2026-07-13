using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.SessionSpec
{
    // Get a single session with its full exercise list (for the session detail page)
    public class SessionWithExercisesSpec : BaseSpecatifications<WorkoutSession>
    {
        public SessionWithExercisesSpec(int sessionId) : base(s => s.Id == sessionId)
        {
            Includes.Add(s => s.ProgramWeek);

            Includes.Add(s => s.SessionExercises);

            IncludeStrings.Add("SessionExercises.Exercise");
        }
    }
}