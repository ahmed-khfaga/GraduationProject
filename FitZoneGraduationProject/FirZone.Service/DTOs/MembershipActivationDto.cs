namespace FitZone.Service.DTOs
{
    public class MembershipActivationDto
    {
        public int MembershipPlanId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
