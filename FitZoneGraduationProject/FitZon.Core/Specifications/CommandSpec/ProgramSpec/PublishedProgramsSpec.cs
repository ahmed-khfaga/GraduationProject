using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{
    // Used by trainees browsing the published catalogue
    public class PublishedProgramsSpec : BaseSpecatifications<WorkoutProgram>
    {
        public PublishedProgramsSpec(ProgramFilterParams p) : base(w =>
            w.IsPublished &&
            (!p.TrackID.HasValue || w.TrackId == p.TrackID) &&
            (!p.Goal.HasValue || w.TrainingGoal == p.Goal) &&
            (!p.Level.HasValue || w.FitnessLevel == p.Level) &&
            (!p.Equipment.HasValue || w.EquipmentType == p.Equipment) &&
            (!p.DurationWeeks.HasValue || w.DurationOnWeeks == p.DurationWeeks))
        {
            Includes.Add(w => w.Coach);
            Includes.Add(w => w.Track);

            if (p.Sort == "newest")
                OrderByDescending = w => w.PublishedAt!;
            else
                OrderBy = w => w.Name;

            ApplyPagination(p.PageIndex, p.PageSize);
        }

        // Count query — no pagination, no includes
        public PublishedProgramsSpec(ProgramFilterParams p, bool countOnly) : base(w =>
            w.IsPublished &&
            (!p.TrackID.HasValue || w.TrackId == p.TrackID) &&
            (!p.Goal.HasValue || w.TrainingGoal == p.Goal) &&
            (!p.Level.HasValue || w.FitnessLevel == p.Level) &&
            (!p.Equipment.HasValue || w.EquipmentType == p.Equipment) &&
            (!p.DurationWeeks.HasValue || w.DurationOnWeeks == p.DurationWeeks))
        {
        }
    }
}
