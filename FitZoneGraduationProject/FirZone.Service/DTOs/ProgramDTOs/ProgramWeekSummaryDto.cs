using FitZone.Service.DTOs.EnrollmentDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProgramDTOs
{
    // Week + session summary inside program detail
    public class ProgramWeekSummaryDto
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public string? WeekDescription { get; set; }
        public string? FocusArea { get; set; }
        //How this week builds on the previous one.
        public string? ProgressionNote { get; set; }

        //Preview of what is coming next week.
        public string? NextWeekPreview { get; set; }
        public int SessionCount { get; set; }

        public List<SessionSummaryDto> Sessions { get; set; } = new();
    }
}
