using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // Single enrollment by id — verifies ownership
    public class EnrollmentByIdSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public EnrollmentByIdSpec(int enrollmentId, int traineeId) : base(e =>
            e.ID == enrollmentId && e.TraineeID == traineeId)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.Track);
        }
    }
}
