using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // All active enrollments for a trainee — up to one per track
    public class TraineeActiveEnrollmentsSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public TraineeActiveEnrollmentsSpec(int traineeId) : base(e =>
            e.TraineeID == traineeId && e.IsActive)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.Track);
        }
    }
}
