using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{

    public class ProgramWeeksByProgramSpec : BaseSpecatifications<ProgramWeek>
    {
        public ProgramWeeksByProgramSpec(int programId) : base(w => w.WorkoutProgramId == programId)
        {
            Includes.Add(w => w.WorkoutSessions);

            OrderBy = w => w.WeekNumber;
        }
    }
}
