using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // Find a previous (inactive) enrollment in the SAME PROGRAM — used to resume saved progress
    public class PreviousEnrollmentInProgramSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public PreviousEnrollmentInProgramSpec(int traineeId, int programId) : base(e =>
            e.TraineeID == traineeId && e.WorkoutProgramID == programId && !e.IsActive)
        {
            Includes.Add(e => e.WorkoutProgram); // required by MapToEnrollmentDto and SyncMaxWeekUnlockedAsync
            Includes.Add(e => e.Track);          // required by MapToEnrollmentDto
            OrderByDescending = e => e.MaxWeekUnlocked; // pick the row with the most saved progress
        }
    }
}
