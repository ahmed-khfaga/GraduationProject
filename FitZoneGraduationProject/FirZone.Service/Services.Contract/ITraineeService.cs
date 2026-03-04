using FitZone.Service.DTOs.ProfileDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.Services.Contract
{
    public interface ITraineeService
    {
        Task<TraineeProfileDto?> GetProfileAsync(string appUserId);
        Task<bool> UpdateProfileAsync(string appUserId, UpdateTraineeProfileDto dto);
    }
}
