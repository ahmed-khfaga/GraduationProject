using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ExerciseSpec
{
    public class ExercisesSpec : BaseSpecatifications<Exercise>
    {
        public ExercisesSpec(ExerciseFilterParams p) : base(e =>
            (!p.Level.HasValue || e.FitnessLevel == p.Level))
        {
            // String-based filters applied at service layer to avoid EF translation issues
            OrderBy = e => e.Name;
            ApplyPagination(p.PageIndex, p.PageSize);
        }

        public ExercisesSpec(int id) : base(e => e.ID == id)
        {
        }

    }
}
