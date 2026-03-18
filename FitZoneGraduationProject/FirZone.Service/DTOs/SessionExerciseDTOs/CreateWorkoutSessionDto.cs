using FitZone.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
    public class CreateWorkoutSessionDto
    {
        public string SessionTitle { get; set; }
        public WeekDay WeekDay { get; set; }
        
        // When a day has more than one session, DayOrder controls the display sequence
        public int DayOrder { get; set; } = 1;
        public int EstimatedDuration { get; set; }
        public string? WarmupNotes { get; set; }
        public string? PrimerNotes { get; set; }
        public string? CooldownNotes { get; set; }
        public List<CreateSessionExerciseDto> CreateSessionExerciseDto { get; set; } = new();
    }
}
