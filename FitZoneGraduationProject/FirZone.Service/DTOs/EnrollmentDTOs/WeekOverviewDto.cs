namespace FitZone.Service.DTOs.EnrollmentDTOs
{
  
    public class WeekOverviewDto
    {
        // the 1-based week number.
        public int WeekNumber { get; set; }

        // Coach-written summary shown on the card (may be null for unseeded programs).
        public string? WeekDescription { get; set; }

        //What muscle group / theme this week focuses on (e.g. "Upper body hypertrophy").
        public string? FocusArea { get; set; }

       
        /// How this week's load builds on the previous one.
        /// Shown on the card even for a locked week so the trainee knows what is coming.
        
        public string? ProgressionNote { get; set; }

        // Coach's teaser for next week — motivational forward-look.
        public string? NextWeekPreview { get; set; }

        // Number of workout sessions in this week.
        public int SessionCount { get; set; }

      
        /// True when the trainee's time-based progress allows access to this week.
        /// Computed from MaxWeekUnlocked; never calculated on the client.
        
        public bool IsUnlocked { get; set; }
    }
}
