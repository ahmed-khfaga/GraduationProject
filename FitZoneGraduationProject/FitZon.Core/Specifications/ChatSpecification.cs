using FitZone.Core.Entitys.Chat;

namespace FitZone.Core.Specifications
{
    // Original spec — only change is adding the ChatType filter so bot rows never appear here
    public class ChatSpecification : BaseSpecatifications<ChatMessage>
    {
        public ChatSpecification(string user1, string user2)
            : base(m =>
                m.ChatType == ChatType.HumanToHuman &&
                ((m.SenderId == user1 && m.ReceiverId == user2) ||
                 (m.SenderId == user2 && m.ReceiverId == user1)))
        {
            Includes.Add(m => m.Sender);
            Includes.Add(m => m.Receiver);
            OrderBy = m => m.SentAt;
        }
    }

    // NEW — all bot turns for one user (used by the history sidebar)
    public class BotHistoryByUserSpec : BaseSpecatifications<ChatMessage>
    {
        public BotHistoryByUserSpec(string userId)
            : base(m =>
                m.ChatType == ChatType.BotToUser &&
                m.SenderId == userId)
        {
            Includes.Add(m => m.Sender);
            OrderBy = m => m.SentAt;
        }
    }

    // NEW — all turns inside ONE specific bot session
    public class BotConversationSpec : BaseSpecatifications<ChatMessage>
    {
        public BotConversationSpec(string userId, Guid conversationId)
            : base(m =>
                m.ChatType == ChatType.BotToUser &&
                m.SenderId == userId &&
                m.BotConversationId == conversationId)
        {
            Includes.Add(m => m.Sender);
            OrderBy = m => m.SentAt;
        }
    }
}