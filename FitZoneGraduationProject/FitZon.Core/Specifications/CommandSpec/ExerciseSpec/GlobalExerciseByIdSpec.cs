using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.ExerciseSpec
{
    // Fetch a single GLOBAL exercise only. Used by admin update/delete — the admin
    // must never be able to modify or remove a coach's private exercise, only the
    // shared global library (CoachId == null).
    public class GlobalExerciseByIdSpec : BaseSpecatifications<Exercise>
    {
        public GlobalExerciseByIdSpec(int exerciseId) : base(e =>
            e.Id == exerciseId && e.CoachId == null)
        {
        }
    }
}