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
    // No class-level [Authorize] — each action declares its own role so that
    // Admin actions are not blocked by a Coach requirement (and vice-versa).
    public class ExerciseController : BaseApiController
    {
        private readonly IExerciseService _exerciseService;
        private readonly ICoachService _coachService;

        public ExerciseController(IExerciseService exerciseService, ICoachService coachService)
        {
            _exerciseService = exerciseService;
            _coachService = coachService;
        }

        // ── Coach endpoints ──────────────────────────────────────────────

        // GET api/exercises  — coach sees global + their own private exercises
        [Authorize(Roles = "Coach")]
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ExerciseSummaryDto>>> GetAll([FromQuery] ExerciseFilterParams filters)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            return Ok(await _exerciseService.GetExercisesForCoachAsync(coachId.Value, filters));
        }

        // GET api/exercises/{id}
        [Authorize(Roles = "Coach")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExerciseDetailDto>> GetById(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var exercise = await _exerciseService.GetExerciseByIdForCoachAsync(id, coachId.Value);
            return exercise is null ? NotFound(new ApiException(404, "Exercise not found.")) : Ok(exercise);
        }

        // POST api/exercises  — coach creates a private exercise
        [Authorize(Roles = "Coach")]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateExerciseDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var id = await _exerciseService.CreateExerciseAsync(dto, coachId.Value);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT api/exercises/{id}  — coach updates their own private exercise
        [Authorize(Roles = "Coach")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateExerciseDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var updated = await _exerciseService.UpdateExerciseAsync(id, dto, coachId.Value);
            return updated ? Ok(new { message = "Exercise updated." })
                           : NotFound(new ApiException(404, "Exercise not found or you cannot edit a global exercise."));
        }

        // DELETE api/exercises/{id}  — coach deletes their own private exercise
        [Authorize(Roles = "Coach")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var deleted = await _exerciseService.DeleteExerciseAsync(id, coachId.Value);
            return deleted ? Ok(new { message = "Exercise deleted." })
                           : NotFound(new ApiException(404, "Exercise not found or you cannot delete a global exercise."));
        }

        private async Task<int?> ResolveCoachIdAsync()
        {
            var profile = await _coachService.GetMyProfileAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return profile?.Id;
        }

        private ActionResult CoachNotFound()
            => Unauthorized(new ApiException(401, "Coach profile not found."));

        // ── Admin endpoints ──────────────────────────────────────────────

        // GET api/exercises/admin/global  — admin browses the shared global library
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/global")]
        public async Task<ActionResult<PaginatedResult<ExerciseSummaryDto>>> AdminGetGlobal([FromQuery] ExerciseFilterParams filters)
            => Ok(await _exerciseService.AdminGetExercisesAsync(isGlobal: true, filters));

        // GET api/exercises/admin/coach-owned  — admin read-only oversight of coach-private exercises
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/coach-owned")]
        public async Task<ActionResult<PaginatedResult<ExerciseSummaryDto>>> AdminGetCoachOwned([FromQuery] ExerciseFilterParams filters)
            => Ok(await _exerciseService.AdminGetExercisesAsync(isGlobal: false, filters));

        // POST api/exercises/admin  — admin creates a new GLOBAL exercise
        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public async Task<ActionResult> AdminCreate([FromBody] CreateExerciseDto dto)
        {
            var id = await _exerciseService.AdminCreateGlobalExerciseAsync(dto);
            return Ok(new { id, message = "Global exercise created." });
        }

        // PUT api/exercises/admin/{id}  — admin updates a GLOBAL exercise only
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id:int}")]
        public async Task<ActionResult> AdminUpdate(int id, [FromBody] CreateExerciseDto dto)
        {
            var updated = await _exerciseService.AdminUpdateGlobalExerciseAsync(id, dto);
            return updated ? Ok(new { message = "Global exercise updated." })
                           : NotFound(new ApiException(404, "Global exercise not found. (Coach-private exercises cannot be edited by an admin.)"));
        }

        // DELETE api/exercises/admin/{id}  — admin deletes a GLOBAL exercise only
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id:int}")]
        public async Task<ActionResult> AdminDelete(int id)
        {
            var deleted = await _exerciseService.AdminDeleteGlobalExerciseAsync(id);
            return deleted ? Ok(new { message = "Global exercise deleted." })
                           : NotFound(new ApiException(404, "Global exercise not found. (Coach-private exercises cannot be deleted by an admin.)"));
        }
    }
}