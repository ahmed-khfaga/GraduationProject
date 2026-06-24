namespace FitZone.Service.DTOs.ChatDTOs
{
    public class ChatAccessDto
    {
        public bool CanChat { get; set; }
        public bool HasPremiumAccess { get; set; }
        public string? Message { get; set; }
    }
}
