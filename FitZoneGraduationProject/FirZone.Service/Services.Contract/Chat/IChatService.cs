using FitZone.Service.DTOs.ChatDTOs;

namespace FitZone.Service.Services.Contract.Chat;

public interface IChatService
{
  
    Task SaveMessageAsync(string senderId, string receiverId, string message);
    Task<IEnumerable<ChatMessageDto>> GetConversation(string user1, string user2);
    Task<bool> CanUsersChatAsync(string user1, string user2);

    
    Task SaveBotTurnAsync(string userId, Guid conversationId, string role, string message);
    Task<IEnumerable<BotMessageDto>> GetBotConversationAsync(string userId, Guid conversationId);
    Task<IEnumerable<BotConversationSummaryDto>> GetBotHistoryAsync(string userId);
}