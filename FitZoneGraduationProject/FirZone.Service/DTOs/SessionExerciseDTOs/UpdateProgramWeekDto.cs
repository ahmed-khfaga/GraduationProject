using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.SessionExerciseDTOs
{
    public class UpdateProgramWeekDto
    {
        public string? WeekDescription { get; set; }
        public string? FocusArea { get; set; }
        public string? ProgressionNote { get; set; }
        public string? NextWeekPreview { get; set; }
    }
}
