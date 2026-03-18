using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProgramDTOs
{
    // Full program detail page
    public class ProgramDetailDto : ProgramCardDto
    {
        public string? NextSteps { get; set; }
        public List<ProgramWeekSummaryDto> ProgramWeekSummaryDto { get; set; } = new();
    }
}
