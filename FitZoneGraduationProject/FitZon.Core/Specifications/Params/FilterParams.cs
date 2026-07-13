using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.Params
{
    public class NutritionPlanFilterParams
    {
        public TrainingGoal? Goal { get; set; }
        public FitnessLevel? Level { get; set; }
        public EquipmentType? Equipment { get; set; }
        public int? DurationWeeks { get; set; }
        public bool? LinkedToProgram { get; set; }
        public string? Sort { get; set; }  // "newest" | default name A-Z
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class FoodItemFilterParams
    {
        public FoodCategory? Category { get; set; }
        public string? Name { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
