using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.ExerciseSpec
{
    // Admin browsing: isGlobal = true  -> CoachId == null  (the shared library admin manages)
    //                 isGlobal = false -> CoachId != null  (read-only oversight of coach-private ones)
    public class ExercisesByGlobalFlagSpec : BaseSpecatifications<Exercise>
    {
        public ExercisesByGlobalFlagSpec(bool isGlobal, ExerciseFilterParams p) : base(e =>
            (isGlobal ? e.CoachId == null : e.CoachId != null) &&
            (!p.Level.HasValue || e.FitnessLevel == p.Level) &&
            (string.IsNullOrWhiteSpace(p.Muscle) || (e.PrimaryMuscles != null && e.PrimaryMuscles.Contains(p.Muscle))) &&
            (string.IsNullOrWhiteSpace(p.Equipment) || (e.EquipmentNeeded != null && e.EquipmentNeeded.Contains(p.Equipment))))
        {
            OrderByDescending = e => e.Id;
            ApplyPagination(p.PageIndex, p.PageSize);
        }

        // Count query — no pagination
        public ExercisesByGlobalFlagSpec(bool isGlobal, ExerciseFilterParams p, bool countOnly) : base(e =>
            (isGlobal ? e.CoachId == null : e.CoachId != null) &&
            (!p.Level.HasValue || e.FitnessLevel == p.Level) &&
            (string.IsNullOrWhiteSpace(p.Muscle) || (e.PrimaryMuscles != null && e.PrimaryMuscles.Contains(p.Muscle))) &&
            (string.IsNullOrWhiteSpace(p.Equipment) || (e.EquipmentNeeded != null && e.EquipmentNeeded.Contains(p.Equipment))))
        {
        }
    }
}