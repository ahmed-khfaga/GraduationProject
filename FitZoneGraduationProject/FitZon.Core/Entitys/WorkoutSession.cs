using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    public class WorkoutSession : BaseEntity
    {

       public int ProgramWeekID {  get; set; }

       public string SessionTitle { get; set; } 

       public WeekDay weekDay { get; set; }

       public int EstimatedDuration { get; set; }


        // For programs that have multiple sessions on the same day this, controls the display order (1 = morning, 2 = afternoon, ..)
        public int DayOrder { get; set; } = 1;
        public string? WarmupNotes { get; set; }

       public string? PrimerNotes { get; set; }

       public string? CooldownNotes { get; set; }

       public virtual ProgramWeek ProgramWeek { get; set; }
       public virtual ICollection<SessionExercise> SessionExercises { get; set; } = new HashSet<SessionExercise>();

    }
}
