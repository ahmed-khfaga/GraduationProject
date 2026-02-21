using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.EnrollmentDTOs
{
    /// Returned on the dashboard — one per active enrollment (up to 4, one per track).
    /// MaxWeekUnlocked tells the client how far the trainee can navigate.
    /// TotalWeeks lets the client render a progress bar.

    public class EnrollmentDTOs
    {
        public int Id { get; set; }
        public int WorkoutProgramID { get; set; }
        public string ProgramName { get; set; }
        public string TrackName { get; set; }
        public int MaxWeekUnlocked { get; set; }   // highest week they can open right now
        public int TotalWeeks { get; set; }   // program's total duration
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
