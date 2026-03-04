using FitZone.Core.Specifications.CommandSpec.ExerciseSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;


namespace FitZone.Service.Services.Contract
{
    public interface IExerciseService
    {
        Task<PaginatedResult<ExerciseSummaryDto>> GetExercisesAsync(ExerciseFilterParams filters);
        Task<ExerciseDetailDto?> GetExerciseByIdAsync(int id);
        Task<int> CreateExerciseAsync(CreateExerciseDto dto);
        Task<bool> UpdateExerciseAsync(int id, CreateExerciseDto dto);
        Task<bool> DeleteExerciseAsync(int id);
    }
}
