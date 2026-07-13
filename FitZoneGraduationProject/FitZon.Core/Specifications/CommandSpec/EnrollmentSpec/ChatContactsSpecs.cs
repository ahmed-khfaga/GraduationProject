using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.EnrollmentSpec
{
    // Trainee view — active workout enrollments with full Coach chain loaded
    public class TraineeActiveEnrollmentsWithCoachSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public TraineeActiveEnrollmentsWithCoachSpec(int traineeId)
            : base(e => e.TraineeId == traineeId && e.IsActive)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.WorkoutProgram.Coach);
            Includes.Add(e => e.WorkoutProgram.Coach.ApplicationUser);
            Includes.Add(e => e.Track);
        }
    }

    // Coach view — all active enrollments across ALL trainees for this coach
    // Loads Trainee + ApplicationUser so we can build trainee cards
    public class ActiveEnrollmentsForCoachSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public ActiveEnrollmentsForCoachSpec(int coachId)
            : base(e => e.WorkoutProgram.CoachId == coachId && e.IsActive)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.WorkoutProgram.Coach);
            Includes.Add(e => e.WorkoutProgram.Coach.ApplicationUser);
            Includes.Add(e => e.Track);
            Includes.Add(e => e.Trainee);
            Includes.Add(e => e.Trainee.ApplicationUser);
        }
    }

    // Coach view — all past (inactive) workout enrollments for one trainee under one coach
    public class TraineeAllEnrollmentsByCoachSpec : BaseSpecatifications<TraineeProgramEnrollment>
    {
        public TraineeAllEnrollmentsByCoachSpec(int traineeId, int coachId)
            : base(e => e.TraineeId == traineeId && e.WorkoutProgram.CoachId == coachId && !e.IsActive)
        {
            Includes.Add(e => e.WorkoutProgram);
            Includes.Add(e => e.Track);
            OrderByDescending = e => e.StartDate;
        }
    }
}