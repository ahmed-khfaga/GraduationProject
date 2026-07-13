using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.SessionSpec
{
    public class SessionExerciseByIdAndCoachSpec : BaseSpecatifications<SessionExercise>
    {
        public SessionExerciseByIdAndCoachSpec(int sessionExerciseId, int coachId)
            : base(se => se.Id == sessionExerciseId
                      && se.WorkoutSession.ProgramWeek.WorkoutProgram.CoachId == coachId)
        {
            Includes.Add(se => se.WorkoutSession);
            IncludeStrings.Add("WorkoutSession.ProgramWeek");
            IncludeStrings.Add("WorkoutSession.ProgramWeek.WorkoutProgram");
        }
    }
}
