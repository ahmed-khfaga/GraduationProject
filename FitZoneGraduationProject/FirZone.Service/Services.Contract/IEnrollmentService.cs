using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.Services.Contract
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId);

        Task<IEnumerable<EnrollmentHistoryDto>> GetMyEnrollmentHistoryAsync(int traineeId);

        
        Task<IEnumerable<WeekOverviewDto>> GetWeekOverviewAsync(int enrollmentId, int traineeId);

        Task<WeekDetailDto?> GetWeekAsync(int enrollmentId, int weekNumber, int traineeId);

        Task<WorkoutSessionDto?> GetSessionDetailAsync(int sessionId, int traineeId);

        Task<EnrollmentDto> StartProgramAsync(int traineeId, StartProgramDto dto);

        Task CancelEnrollmentAsync(int enrollmentId, int traineeId);
    }
}
