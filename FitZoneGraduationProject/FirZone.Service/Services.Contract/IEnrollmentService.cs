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
        // Dashboard — active enrollments only, week unlock computed on read
        Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId);

        // History page — all enrollments including inactive ones
        Task<IEnumerable<EnrollmentHistoryDto>> GetMyEnrollmentHistoryAsync(int traineeId);

        // View a specific week inside an enrollment (access gated by MaxWeekUnlocked)
        Task<WeekDetailDto?> GetWeekAsync(int enrollmentId, int weekNumber, int traineeId);

        // Full session detail with section-grouped exercises (access gated)
        Task<WorkoutSessionDto?> GetSessionDetailAsync(int sessionId, int traineeId);

        // Start or resume a program
        Task<EnrollmentDto> StartProgramAsync(int traineeId, StartProgramDto dto);

        // Cancel — row kept, progress preserved for future resume
        Task CancelEnrollmentAsync(int enrollmentId, int traineeId);
    }
}
