using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
    public class WorkoutSessionDTOs
    {
        public int Id { get; set; }
        public string SessionTitle { get; set; }
        public string WeekDay { get; set; }
        public int EstimatedDuration { get; set; }
        public string? WarmupNotes { get; set; }
        public string? PrimerNotes { get; set; }
        public string? CooldownNotes { get; set; }
        public List<SessionExerciseDTOs> SessionExerciseDTOs { get; set; } = new();
    }
}
