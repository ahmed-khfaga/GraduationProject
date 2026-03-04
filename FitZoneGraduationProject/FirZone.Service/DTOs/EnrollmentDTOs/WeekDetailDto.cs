
namespace FitZone.Service.DTOs.EnrollmentDTOs
{
  
    /// Returned when viewing a specific week inside an enrollment.
    /// Lists all sessions for that week in day order.
   
    public class WeekDetailDto
    {
        public int WeekNumber { get; set; }
        public string? WeekDescription { get; set; }
        public string? FocusArea { get; set; }
        public bool IsUnlocked { get; set; }
        public List<SessionSummaryDto> SessionSummaryDto { get; set; } = new();
    }
}
