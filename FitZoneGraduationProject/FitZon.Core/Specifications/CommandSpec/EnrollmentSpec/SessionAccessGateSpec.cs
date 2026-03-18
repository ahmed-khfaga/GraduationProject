using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // Verify a session belongs to a week that is unlocked for this trainee
    // Used as an access gate before returning session detail
    public class SessionAccessGateSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public SessionAccessGateSpec(int traineeId, int programId) : base(e =>
            e.TraineeID == traineeId && e.WorkoutProgramID == programId)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.Track);          
        }
    }
}
