using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    public class Exercise :BaseEntity
    {
        public string Name {  get; set; }

        public string? Description { get; set; }

        public string? PrimaryMuscles { get; set; }

        public string? SecondaryMuscles { get; set; }

        public string? EquipmentNeeded { get; set; }

        public FitnessLevel FitnessLevel { get; set; }


        public string? VideoUrl { get; set; }

        public string? Instructions { get; set; }

        public string? CommonMistakes { get; set; }


        public virtual ICollection<SessionExercise> SessionExercises { get; set; } = new HashSet<SessionExercise>();




    }
}
