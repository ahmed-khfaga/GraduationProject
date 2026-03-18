using FitZone.Core.Specifications.CommandSpec.ExerciseSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.APIs.Controllers
{
    [Authorize(Roles = "Coach")]
    public class ExerciseController : BaseApiController
    {
        private readonly IExerciseService _exerciseService;
        private readonly ICoachService _coachService;

        public ExerciseController(IExerciseService exerciseService, ICoachService coachService)
        {
            _exerciseService = exerciseService;
            _coachService = coachService;
        }

        // GET api/exercises  — coach sees global + their own private exercises
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ExerciseSummaryDto>>> GetAll([FromQuery] ExerciseFilterParams filters)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            return Ok(await _exerciseService.GetExercisesForCoachAsync(coachId.Value, filters));
        }

        // GET api/exercises/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExerciseDetailDto>> GetById(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var exercise = await _exerciseService.GetExerciseByIdForCoachAsync(id, coachId.Value);
            return exercise is null ? NotFound(new ApiException(404, "Exercise not found.")) : Ok(exercise);
        }

        // POST api/exercises  — coach creates a private exercise
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateExerciseDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var id = await _exerciseService.CreateExerciseAsync(dto, coachId.Value);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT api/exercises/{id}  — coach updates their own private exercise
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateExerciseDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var updated = await _exerciseService.UpdateExerciseAsync(id, dto, coachId.Value);
            return updated ? Ok(new { message = "Exercise updated." }) : NotFound(new ApiException(404, "Exercise not found or you cannot edit a global exercise."));
        }

        // DELETE api/exercises/{id}  — coach deletes their own private exercise
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var deleted = await _exerciseService.DeleteExerciseAsync(id, coachId.Value);
            return deleted ? Ok(new { message = "Exercise deleted." }) : NotFound(new ApiException(404, "Exercise not found or you cannot delete a global exercise."));
        }

        private async Task<int?> ResolveCoachIdAsync()
        {
            var profile = await _coachService.GetMyProfileAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return profile?.Id;
        }

        private ActionResult CoachNotFound()
            => Unauthorized(new ApiException(401, "Coach profile not found."));
    }

}
