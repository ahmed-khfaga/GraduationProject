using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProgramSpec
{
    public class ProgramFilterParams
    {
        public int? TrackID { get; set; }
        public TrainingGoal? Goal { get; set; }
        public FitnessLevel? Level { get; set; }
        public EquipmentType? Equipment { get; set; }
        public int? DurationWeeks { get; set; }
        public string? Sort { get; set; }   // "newest" | "popular"
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
