using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ExerciseSpec
{
    public class ExerciseFilterParams
    {
        public FitnessLevel? Level { get; set; }
        public string? Muscle { get; set; }         // partial match on PrimaryMuscles
        public string? Equipment { get; set; }       // partial match on EquipmentNeeded
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
