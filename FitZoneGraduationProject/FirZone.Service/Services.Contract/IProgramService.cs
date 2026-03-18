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
        // Public catalogue
        Task<PaginatedResult<ProgramCardDto>> GetPublishedProgramsAsync(ProgramFilterParams filters);
        Task<ProgramDetailDto?> GetProgramDetailAsync(int programId);

        // Coach's own programs (all, published + unpublished)
        Task<IEnumerable<ProgramCardDto>> GetCoachProgramsAsync(int coachId);

        // ── Coach full control ─────

        Task<int> CreateProgramAsync(int coachId, CreateProgramDto dto);

        Task<bool> UpdateProgramAsync(int programId, int coachId, UpdateProgramDto dto);

        Task AddProgramWeekAsync(int programId, int coachId, CreateProgramWeekDto dto);

        Task<bool> UpdateProgramWeekAsync(int programWeekId, int coachId, UpdateProgramWeekDto dto);

        Task<bool> DeleteProgramWeekAsync(int programWeekId, int coachId);
  
        Task<bool> UpdateSessionAsync(int sessionId, int coachId, UpdateWorkoutSessionDto dto);
      
        Task<bool> DeleteSessionAsync(int sessionId, int coachId);

        // Coach publishes their own program — immediately visible in catalogue
        Task<bool> PublishProgramAsync(int programId, int coachId);

        // Coach unpublishes (hides from catalogue — enrolled trainees unaffected)
        Task<bool> UnpublishProgramAsync(int programId, int coachId);

        Task<bool> DeleteProgramAsync(int programId, int coachId);

        // Admin hard-deletes any program
        Task<bool> AdminDeleteProgramAsync(int programId);
    }
}
