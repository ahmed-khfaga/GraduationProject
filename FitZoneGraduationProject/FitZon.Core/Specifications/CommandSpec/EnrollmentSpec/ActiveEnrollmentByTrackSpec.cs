using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // Active enrollment for a trainee in a specific track (one at a time)
    public class ActiveEnrollmentByTrackSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public ActiveEnrollmentByTrackSpec(int traineeId, int trackId) : base(e =>
            e.TraineeId == traineeId && e.TrackId == trackId && e.IsActive)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.Track);
        }
    }
}
