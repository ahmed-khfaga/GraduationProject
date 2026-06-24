using System.ComponentModel.DataAnnotations;

namespace FitZone.Service.DTOs.PaymentDTOs
{
    public class ConfirmPaymentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card number is required.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits.")]
        public string CardNumber { get; set; } = string.Empty;
    }
}
