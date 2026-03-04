using FitZone.Core.Specifications.CommandSpec.ExerciseSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    public class ExerciseController : BaseApiController
    {
        private readonly IExerciseService _exerciseService;

        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        // GET api/exercises
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ExerciseSummaryDto>>> GetAll([FromQuery] ExerciseFilterParams filters)
        {
            var result = await _exerciseService.GetExercisesAsync(filters);
            return Ok(result);
        }

        // GET api/exercises/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExerciseDetailDto>> GetById(int id)
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            if (exercise is null)
                return NotFound(new ApiException(404, "Exercise not found."));

            return Ok(exercise);
        }

        // POST api/exercises  — Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CreateExerciseDto dto)
        {
            var id = await _exerciseService.CreateExerciseAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT api/exercises/{id}  — Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateExerciseDto dto)
        {
            var updated = await _exerciseService.UpdateExerciseAsync(id, dto);
            if (!updated)
                return NotFound(new ApiException(404, "Exercise not found."));

            return Ok(new { message = "Exercise updated." });
        }

        // DELETE api/exercises/{id}  — Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _exerciseService.DeleteExerciseAsync(id);
            if (!deleted)
                return NotFound(new ApiException(404, "Exercise not found."));

            return Ok(new { message = "Exercise deleted." });
        }
    }
}
