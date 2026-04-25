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
        public int WorkoutProgramId { get; set; }

        public int WeekNumber { get; set; }


        public string? WeekDescription {  get; set; }

        public string? FocusArea { get; set; }

       
        // Coach's message explaining how this week progresses from the previous one.
        // (Optional — not enforced at DB level, but strongly encouraged in the UI.)
        public string? ProgressionNote { get; set; }

        
        // Coach's note on what to expect in the NEXT week — forward-looking motivation.
        public string? NextWeekPreview { get; set; }

        public virtual WorkoutProgram WorkoutProgram { get; set; }
        public virtual ICollection<WorkoutSession> WorkoutSessions { get; set; } = new HashSet<WorkoutSession>();

    }
}
