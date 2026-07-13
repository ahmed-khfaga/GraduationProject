using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.SessionSpec
{
    // Loads one session + its exercises, verified by coach ownership 
    public class WorkoutSessionFullByIdAndCoachSpec : BaseSpecatifications<WorkoutSession>
    {
        public WorkoutSessionFullByIdAndCoachSpec(int sessionId, int coachId)
            : base(s => s.Id == sessionId
                     && s.ProgramWeek.WorkoutProgram.CoachId == coachId)
        {
            Includes.Add(s => s.ProgramWeek);
            IncludeStrings.Add("ProgramWeek.WorkoutProgram");
            IncludeStrings.Add("SessionExercises");
            IncludeStrings.Add("SessionExercises.Exercise");
        }
    }
}