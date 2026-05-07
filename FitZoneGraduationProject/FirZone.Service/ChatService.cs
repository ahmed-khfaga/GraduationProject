using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // Chat is only allowed between a trainee and a coach that owns one of trainee's active enrollments.
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
}
