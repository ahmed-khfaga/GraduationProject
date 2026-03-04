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
        public List<CreateWorkoutSessionDto> CreateWorkoutSessionDto { get; set; } = new();
    }
}
