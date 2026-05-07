namespace FitZone.Service.DTOs.PaymentDTOs
{
    public class PaymentIntentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public int MembershipPlanId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string Status { get; set; } = "Pending";
    }
}
