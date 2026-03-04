using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // ALL enrollments for a trainee — active AND historical (history page)
    public class TraineeAllEnrollmentsSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public TraineeAllEnrollmentsSpec(int traineeId) : base(e => e.TraineeID == traineeId)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.Track);
            OrderByDescending = e => e.StartDate;
        }
    }
}
