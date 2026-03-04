using FitZone.Core.Specifications.CommandSpec.ProgramSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.ProgramDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.Services.Contract
{
    public interface IProgramService
    {
        Task<PaginatedResult<ProgramCardDto>> GetPublishedProgramsAsync(ProgramFilterParams filters);
        Task<ProgramDetailDto?> GetProgramDetailAsync(int programId);
        Task<IEnumerable<ProgramCardDto>> GetCoachProgramsAsync(int coachId);
        Task<IEnumerable<ProgramCardDto>> GetPendingProgramsAsync();

        // Coach creates a draft program shell
        Task<int> CreateProgramAsync(int coachId, CreateProgramDto dto);

        // Coach adds a week with sessions/exercises
        Task AddProgramWeekAsync(int programId, int coachId, CreateProgramWeekDto dto);

        // Coach submits for review
        Task SubmitForReviewAsync(int programId, int coachId);

        // Admin approves or rejects
        Task ReviewProgramAsync(int programId, AdminReviewDto dto);
    }
}
