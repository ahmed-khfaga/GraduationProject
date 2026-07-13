using FitZone.Service.DTOs.ChatContactsDTOs;

namespace FitZone.Service.Services.Contract
{
    public interface IChatContactsService
    {
        Task<TraineeChatContactsDto> GetContactsForTraineeAsync(string appUserId);
        Task<CoachChatContactsDto> GetContactsForCoachAsync(string appUserId);
    }
}