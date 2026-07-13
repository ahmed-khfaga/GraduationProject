namespace FitZone.Service.DTOs.ChatDTOs
{
    // Existing — unchanged
    public class ChatMessageDto
    {
        public string SenderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
    }

    // NEW — what the client sends when chatting with the bot
    public class SendBotMessageDto
    {
        public Guid BotConversationId { get; set; }  // client generates this Guid per session
        public string Message { get; set; }
    }

    // NEW — one turn returned from the API (user prompt or assistant reply)
    public class BotMessageDto
    {
        public int Id { get; set; }
        public Guid BotConversationId { get; set; }
        public string BotRole { get; set; }   // "user" | "assistant"
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
    }

    // NEW — summary card per session for the history sidebar
    public class BotConversationSummaryDto
    {
        public Guid BotConversationId { get; set; }
        public string FirstMessage { get; set; }   // used as the conversation title
        public DateTime StartedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int TurnCount { get; set; }
    }
}