namespace FitZone.Service.DTOs.PaymentDTOs
{
    public class PaymentStatusDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CardLastFour { get; set; }
    }
}
