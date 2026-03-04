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

        public ExerciseService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ExerciseSummaryDto>> GetExercisesAsync(ExerciseFilterParams filters)
        {
            var spec = new ExercisesSpec(filters);
            var all = await _uow.Repository<Exercise>().GetAllWithSpecAsync(spec);
            var filtered = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filters.Muscle))
                filtered = filtered.Where(e =>
                    e.PrimaryMuscles != null &&
                    e.PrimaryMuscles.Contains(filters.Muscle, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filters.Equipment))
                filtered = filtered.Where(e =>
                    e.EquipmentNeeded != null &&
                    e.EquipmentNeeded.Contains(filters.Equipment, StringComparison.OrdinalIgnoreCase));

            var list = filtered.ToList();

            return new PaginatedResult<ExerciseSummaryDto>
            {
                PageIndex = filters.PageIndex,
                PageSize = filters.PageSize,
                TotalCount = list.Count,
                Data = _mapper.Map<IEnumerable<ExerciseSummaryDto>>(list)
            };
        }

        public async Task<ExerciseDetailDto?> GetExerciseByIdAsync(int id)
        {
            var exercise = await _uow.Repository<Exercise>().GetAsync(id);
            return exercise is null ? null : _mapper.Map<ExerciseDetailDto>(exercise);
        }

        public async Task<int> CreateExerciseAsync(CreateExerciseDto dto)
        {
            var exercise = _mapper.Map<Exercise>(dto);
            _uow.Repository<Exercise>().Add(exercise);
            await _uow.CompleteAsync();
            return exercise.ID;
        }

        public async Task<bool> UpdateExerciseAsync(int id, CreateExerciseDto dto)
        {
            var exercise = await _uow.Repository<Exercise>().GetAsync(id);
            if (exercise is null) return false;

            _mapper.Map(dto, exercise);
            _uow.Repository<Exercise>().Update(exercise);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteExerciseAsync(int id)
        {
            var exercise = await _uow.Repository<Exercise>().GetAsync(id);
            if (exercise is null) return false;

            _uow.Repository<Exercise>().Delete(exercise);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
