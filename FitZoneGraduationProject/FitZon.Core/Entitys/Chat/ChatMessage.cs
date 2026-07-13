using FitZone.Core.Comman;
using FitZone.Core.Entitys.Identity;

namespace FitZone.Core.Entitys.Chat
{
    public class ChatMessage : BaseEntity
    {
        public string SenderId { get; set; }
        public string? ReceiverId { get; set; }        // nullable — null for bot messages

        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public ChatType ChatType { get; set; } = ChatType.HumanToHuman;

        public Guid? BotConversationId { get; set; }  // groups all turns of one bot session
        public string? BotRole { get; set; }           // "user" | "assistant"

        public ApplicationUser Sender { get; set; }
        public ApplicationUser? Receiver { get; set; }
    }
}