using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Entitys.Chat;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications;
using FitZone.Core.Specifications.CommandSpec.EnrollmentSpec;
using FitZone.Core.Specifications.CommandSpec.ProfileSpec;
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

    // ── Human↔Human (original, only added ChatType = HumanToHuman) ──────────────

    public async Task<IEnumerable<ChatMessageDto>> GetConversation(string user1, string user2)
    {
        var spec = new ChatSpecification(user1, user2);
        var messages = await _unitOfWork.Repository<ChatMessage>().GetAllWithSpecAsync(spec);
        return _mapper.Map<IEnumerable<ChatMessageDto>>(messages);
    }

    public async Task SaveMessageAsync(string senderId, string receiverId, string message)
    {
        var chat = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Message = message,
            ChatType = ChatType.HumanToHuman   // explicit, was implicit before
        };
        _unitOfWork.Repository<ChatMessage>().Add(chat);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<bool> CanUsersChatAsync(string user1, string user2)
    {
        if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
            return false;

        var trainee1 = await _unitOfWork.Repository<Trainee>()
            .GetWithSpecAsync(new TraineeByUserIdSpec(user1));
        var coach1 = await _unitOfWork.Repository<Coach>()
            .GetWithSpecAsync(new CoachByUserIdSpec(user1));

        var trainee2 = await _unitOfWork.Repository<Trainee>()
            .GetWithSpecAsync(new TraineeByUserIdSpec(user2));
        var coach2 = await _unitOfWork.Repository<Coach>()
            .GetWithSpecAsync(new CoachByUserIdSpec(user2));

        if (trainee1 is not null && coach2 is not null)
        {
            var enrollments = await _unitOfWork.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(new TraineeActiveEnrollmentsSpec(trainee1.Id));
            return enrollments.Any(e => e.WorkoutProgram.CoachId == coach2.Id);
        }

        if (trainee2 is not null && coach1 is not null)
        {
            var enrollments = await _unitOfWork.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(new TraineeActiveEnrollmentsSpec(trainee2.Id));
            return enrollments.Any(e => e.WorkoutProgram.CoachId == coach1.Id);
        }

        return false;
    }

    // ── Bot history (NEW) ────────────────────────────────────────────────────────

    public async Task SaveBotTurnAsync(string userId, Guid conversationId, string role, string message)
    {
        var turn = new ChatMessage
        {
            SenderId = userId,
            ReceiverId = null,
            Message = message,
            ChatType = ChatType.BotToUser,
            BotConversationId = conversationId,
            BotRole = role
        };
        _unitOfWork.Repository<ChatMessage>().Add(turn);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<BotMessageDto>> GetBotConversationAsync(string userId, Guid conversationId)
    {
        var spec = new BotConversationSpec(userId, conversationId);
        var messages = await _unitOfWork.Repository<ChatMessage>().GetAllWithSpecAsync(spec);
        return _mapper.Map<IEnumerable<BotMessageDto>>(messages);
    }

    public async Task<IEnumerable<BotConversationSummaryDto>> GetBotHistoryAsync(string userId)
    {
        var spec = new BotHistoryByUserSpec(userId);
        var turns = await _unitOfWork.Repository<ChatMessage>().GetAllWithSpecAsync(spec);

        return turns
            .GroupBy(t => t.BotConversationId!.Value)
            .Select(g =>
            {
                var firstUserMsg = g.Where(t => t.BotRole == "user")
                                    .OrderBy(t => t.SentAt)
                                    .FirstOrDefault();
                return new BotConversationSummaryDto
                {
                    BotConversationId = g.Key,
                    FirstMessage = firstUserMsg?.Message ?? "(empty)",
                    StartedAt = g.Min(t => t.SentAt),
                    LastMessageAt = g.Max(t => t.SentAt),
                    TurnCount = g.Count()
                };
            })
            .OrderByDescending(s => s.LastMessageAt)
            .ToList();
    }
}