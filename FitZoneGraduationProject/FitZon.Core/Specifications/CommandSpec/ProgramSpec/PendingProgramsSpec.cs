using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{
    // Programs pending admin review
    public class PendingProgramsSpec : BaseSpecatifications<WorkoutProgram>
    {
        public PendingProgramsSpec() : base(w => w.Status == ProgramStatus.PendingReview)
        {
            Includes.Add(w => w.Coach);
            Includes.Add(w => w.Track);
            OrderBy = w => w.ID;
        }
    }
}
