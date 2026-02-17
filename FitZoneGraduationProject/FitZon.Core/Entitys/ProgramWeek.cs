using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Entitys
{
    public class ProgramWeek :BaseEntity
    {
        public int WorkoutProgramID { get; set; }

        public int WeekNumber { get; set; }


        public string? WeekDescription {  get; set; }

        public string? FocusArea { get; set; }


        public virtual WorkoutProgram WorkoutProgram { get; set; }

    }
}
