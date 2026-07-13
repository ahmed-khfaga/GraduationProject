using FitZone.Core.Specifications.CommandSpec.ExerciseSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;


namespace FitZone.Service.Services.Contract
{
    public interface IExerciseService
    {
        // Coaches browse their own library (global + private)
        Task<PaginatedResult<ExerciseSummaryDto>> GetExercisesForCoachAsync(int coachId, ExerciseFilterParams filters);
        Task<ExerciseDetailDto?> GetExerciseByIdForCoachAsync(int id, int coachId);

        Task<int> CreateExerciseAsync(CreateExerciseDto dto, int coachId);

        Task<bool> UpdateExerciseAsync(int id, CreateExerciseDto dto, int coachId);

        Task<bool> DeleteExerciseAsync(int id, int coachId);

        // ── Admin exercise management ────────────────────────────────────
        // Admin can fully manage the GLOBAL library (create/update/delete) and
        // browse coach-private exercises read-only, but can never modify a coach's
        // private exercise — only the coach who owns it can.
        Task<PaginatedResult<ExerciseSummaryDto>> AdminGetExercisesAsync(bool isGlobal, ExerciseFilterParams filters);
        Task<int> AdminCreateGlobalExerciseAsync(CreateExerciseDto dto);
        Task<bool> AdminUpdateGlobalExerciseAsync(int id, CreateExerciseDto dto);
        Task<bool> AdminDeleteGlobalExerciseAsync(int id);
    }
}