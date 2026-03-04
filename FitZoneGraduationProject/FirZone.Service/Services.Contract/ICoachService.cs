using FitZone.Service.DTOs.ProfileDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.Services.Contract
{
    public interface ICoachService
    {
        Task<IEnumerable<CoachProfileDto>> GetAllCoachesAsync();
        Task<CoachProfileDto?> GetCoachByIdAsync(int coachId);
        Task<CoachProfileDto?> GetMyProfileAsync(string appUserId);
        Task<bool> UpdateMyProfileAsync(string appUserId, UpdateCoachProfileDto dto);
    }
}
