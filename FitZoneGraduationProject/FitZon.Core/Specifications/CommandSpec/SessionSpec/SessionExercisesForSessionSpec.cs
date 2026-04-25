using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.SessionSpec
{
    // Session exercises with exercise detail — used to build the full session view
    public class SessionExercisesForSessionSpec : BaseSpecatifications<SessionExercise>
    {
        public SessionExercisesForSessionSpec(int sessionId) : base(se => se.WorkoutSessionId == sessionId)
        {
            Includes.Add(se => se.Exercise);
        }
    }
}
