using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.ExerciseSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service
{
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ExerciseService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<PaginatedResult<ExerciseSummaryDto>> GetExercisesForCoachAsync(int coachId, ExerciseFilterParams filters)
        {
            filters.PageIndex = filters.PageIndex < 1 ? 1 : filters.PageIndex;
            filters.PageSize = filters.PageSize < 1 ? 20 : Math.Min(filters.PageSize, 100);

            var spec = new ExercisesForCoachSpec(coachId, filters);
            var countSpec = new ExercisesForCoachSpec(coachId, filters, countOnly: true);

            var exercises = await _uow.Repository<Exercise>().GetAllWithSpecAsync(spec);
            var total = await _uow.Repository<Exercise>().CountAsync(countSpec);

            return new PaginatedResult<ExerciseSummaryDto>
            {
                PageIndex = filters.PageIndex,
                PageSize = filters.PageSize,
                TotalCount = total,
                Data = _mapper.Map<IEnumerable<ExerciseSummaryDto>>(exercises)
            };
        }

        public async Task<ExerciseDetailDto?> GetExerciseByIdForCoachAsync(int id, int coachId)
        {
            var spec = new ExerciseByIdForCoachSpec(id, coachId);
            var exercise = await _uow.Repository<Exercise>().GetWithSpecAsync(spec);
            return exercise is null ? null : _mapper.Map<ExerciseDetailDto>(exercise);
        }

        public async Task<int> CreateExerciseAsync(CreateExerciseDto dto, int coachId)
        {
            var exercise = _mapper.Map<Exercise>(dto);
            exercise.CoachId = coachId;             // always private to the creating coach
            _uow.Repository<Exercise>().Add(exercise);
            await _uow.CompleteAsync();
            return exercise.Id;
        }

        public async Task<bool> UpdateExerciseAsync(int id, CreateExerciseDto dto, int coachId)
        {
            // Coaches can only edit their own private exercises — global ones are protected
            var spec = new ExerciseByIdForCoachSpec(id, coachId);
            var exercise = await _uow.Repository<Exercise>().GetWithSpecAsync(spec);
            if (exercise is null || exercise.CoachId is null) return false; // null = global — read-only

            _mapper.Map(dto, exercise);
            _uow.Repository<Exercise>().Update(exercise);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteExerciseAsync(int id, int coachId)
        {
            var spec = new ExerciseByIdForCoachSpec(id, coachId);
            var exercise = await _uow.Repository<Exercise>().GetWithSpecAsync(spec);
            if (exercise is null || exercise.CoachId is null) return false; // global — cannot delete

            _uow.Repository<Exercise>().Delete(exercise);
            await _uow.CompleteAsync();
            return true;
        }

        // ── Admin exercise management ────────────────────────────────────

        public async Task<PaginatedResult<ExerciseSummaryDto>> AdminGetExercisesAsync(bool isGlobal, ExerciseFilterParams filters)
        {
            filters.PageIndex = filters.PageIndex < 1 ? 1 : filters.PageIndex;
            filters.PageSize = filters.PageSize < 1 ? 20 : Math.Min(filters.PageSize, 100);

            var spec = new ExercisesByGlobalFlagSpec(isGlobal, filters);
            var countSpec = new ExercisesByGlobalFlagSpec(isGlobal, filters, countOnly: true);

            var exercises = await _uow.Repository<Exercise>().GetAllWithSpecAsync(spec);
            var total = await _uow.Repository<Exercise>().CountAsync(countSpec);

            return new PaginatedResult<ExerciseSummaryDto>
            {
                PageIndex = filters.PageIndex,
                PageSize = filters.PageSize,
                TotalCount = total,
                Data = _mapper.Map<IEnumerable<ExerciseSummaryDto>>(exercises)
            };
        }

        public async Task<int> AdminCreateGlobalExerciseAsync(CreateExerciseDto dto)
        {
            var exercise = _mapper.Map<Exercise>(dto);
            exercise.CoachId = null; // always global when created by admin
            _uow.Repository<Exercise>().Add(exercise);
            await _uow.CompleteAsync();
            return exercise.Id;
        }

        public async Task<bool> AdminUpdateGlobalExerciseAsync(int id, CreateExerciseDto dto)
        {
            // Scoped to GlobalExerciseByIdSpec — admin can never touch a coach-private exercise.
            var spec = new GlobalExerciseByIdSpec(id);
            var exercise = await _uow.Repository<Exercise>().GetWithSpecAsync(spec);
            if (exercise is null) return false;

            _mapper.Map(dto, exercise);
            _uow.Repository<Exercise>().Update(exercise);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> AdminDeleteGlobalExerciseAsync(int id)
        {
            var spec = new GlobalExerciseByIdSpec(id);
            var exercise = await _uow.Repository<Exercise>().GetWithSpecAsync(spec);
            if (exercise is null) return false;

            _uow.Repository<Exercise>().Delete(exercise);
            await _uow.CompleteAsync();
            return true;
        }
    }
}