using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProgramDTOs
{
    // Full program detail page
    public class ProgramDetailDTOs : ProgramCardDTOs
    {
        public List<ProgramWeekSummaryDTOs> ProgramWeekSummaryDTOs { get; set; } = new();
    }
}
