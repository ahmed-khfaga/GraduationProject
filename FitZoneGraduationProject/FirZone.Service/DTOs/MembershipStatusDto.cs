namespace FitZone.Service.DTOs
{
    public class MembershipStatusDto
    {
        public bool IsActive { get; set; }
        public int? MembershipPlanId { get; set; }
        public string? PlanTitle { get; set; }
        public string? MembershipName { get; set; }
        public bool IsPremium { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
