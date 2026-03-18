using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
    public class CreateProgramWeekDto
    {
        public int WeekNumber { get; set; }
        public string? WeekDescription { get; set; }
        public string? FocusArea { get; set; }

        // How this week's load/volume/intensity progresses from the previous week,  Encouraged but optional — the UI should surface this prominently.
        public string? ProgressionNote { get; set; }

        // A forward-looking message telling the trainee what to expect next week.
        public string? NextWeekPreview { get; set; }
        public List<CreateWorkoutSessionDto> CreateWorkoutSessionDto { get; set; } = new();
    }
}
