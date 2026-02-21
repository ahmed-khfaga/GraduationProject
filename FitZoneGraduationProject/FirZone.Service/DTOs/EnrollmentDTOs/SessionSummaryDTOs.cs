using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.EnrollmentDTOs
{
   
    /// A single session card shown inside a week view — not the full exercise list.
    /// The trainee clicks through to get the full session detail.
   
    public class SessionSummaryDTOs
    {
        public int Id { get; set; }
        public string SessionTitle { get; set; }
        public string WeekDay { get; set; }
        public int EstimatedDuration { get; set; }
    }
}
