using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FitZone.Core.Entitys.Chat;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications;
using FitZone.Service.DTOs.ChatDTOs;
using FitZone.Service.Services.Contract.Chat;

namespace FitZone.Service;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    public async Task<IEnumerable<ChatMessageDto>> GetConversation(string user1, string user2)
    {
        var spec = new ChatSpecification(user1, user2);

        var messages = await _unitOfWork.Repository<ChatMessage>()
            .GetAllWithSpecAsync(spec);

        return _mapper.Map<IEnumerable<ChatMessageDto>>(messages);
    }
    public async Task SaveMessageAsync(string senderId, string receiverId, string message)
    {
        var chat = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Message = message
        };

        _unitOfWork.Repository<ChatMessage>().Add(chat);
        await _unitOfWork.CompleteAsync();
    }
}
