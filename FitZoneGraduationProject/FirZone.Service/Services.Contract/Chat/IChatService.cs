using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys.Chat;
using FitZone.Service.DTOs.ChatDTOs;

namespace FitZone.Service.Services.Contract.Chat;

public interface IChatService
{
    Task SaveMessageAsync(string senderId, string receiverId, string message);
    Task<IEnumerable<ChatMessageDto>> GetConversation(string user1, string user2);
}
