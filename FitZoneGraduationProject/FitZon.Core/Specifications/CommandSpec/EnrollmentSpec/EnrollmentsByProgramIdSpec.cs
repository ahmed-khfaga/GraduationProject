using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    public class EnrollmentsByProgramIdSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public EnrollmentsByProgramIdSpec(int programId)
            : base(e => e.WorkoutProgramId == programId)
        {
        }
    }
}