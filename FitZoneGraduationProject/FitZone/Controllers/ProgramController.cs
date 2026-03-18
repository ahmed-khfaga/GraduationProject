using FitZone.Core.Specifications.CommandSpec.ProgramSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.ProgramDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.APIs.Controllers
{
    public class ProgramController : BaseApiController
    {
        private readonly IProgramService _programService;
        private readonly ICoachService _coachService;

        public ProgramController(IProgramService programService, ICoachService coachService)
        {
            _programService = programService;
            _coachService = coachService;
        }

        // ── Public catalogue ─────────

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ProgramCardDto>>> GetAll([FromQuery] ProgramFilterParams filters)
            => Ok(await _programService.GetPublishedProgramsAsync(filters));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProgramDetailDto>> GetById(int id)
        {
            var program = await _programService.GetProgramDetailAsync(id);
            return program is null ? NotFound(new ApiException(404, "Program not found.")) : Ok(program);
        }

        [HttpGet("coach/{coachId:int}")]
        public async Task<ActionResult<IEnumerable<ProgramCardDto>>> GetByCoach(int coachId)
            => Ok(await _programService.GetCoachProgramsAsync(coachId));

        // GET api/programs/mine  —  coach sees ALL their programs (published + draft)
        [HttpGet("mine")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult<IEnumerable<ProgramCardDto>>> GetMyPrograms()
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var programs = await _programService.GetCoachProgramsAsync(coachId.Value);
            return Ok(programs);
        }

        // ── Coach — create & manage ───────────────

        // POST api/programs  — create shell
        [HttpPost]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Create([FromBody] CreateProgramDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var id = await _programService.CreateProgramAsync(coachId.Value, dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT api/programs/{id}  — edit 
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateProgramDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            await _programService.UpdateProgramAsync(id, coachId.Value, dto);
            return Ok(new { message = "Program updated." });
        }

        // POST api/programs/{id}/weeks  — add a week with sessions
        [HttpPost("{id:int}/weeks")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> AddWeek(int id, [FromBody] CreateProgramWeekDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            await _programService.AddProgramWeekAsync(id, coachId.Value, dto);
            return Ok(new { message = $"Week {dto.WeekNumber} added." });
        }

        // PUT api/programs/weeks/{weekId}  — edit week 
        [HttpPut("weeks/{weekId:int}")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> UpdateWeek(int weekId, [FromBody] UpdateProgramWeekDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var updated = await _programService.UpdateProgramWeekAsync(weekId, coachId.Value, dto);
            return updated ? Ok(new { message = "Week updated." }) : NotFound(new ApiException(404, "Week not found."));
        }

        // DELETE api/programs/weeks/{weekId}  — delete a week
        [HttpDelete("weeks/{weekId:int}")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> DeleteWeek(int weekId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var deleted = await _programService.DeleteProgramWeekAsync(weekId, coachId.Value);
            return deleted ? Ok(new { message = "Week deleted." }) : NotFound(new ApiException(404, "Week not found."));
        }

        // PUT api/programs/sessions/{sessionId}  — edit session 
        [HttpPut("sessions/{sessionId:int}")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> UpdateSession(int sessionId, [FromBody] UpdateWorkoutSessionDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var updated = await _programService.UpdateSessionAsync(sessionId, coachId.Value, dto);
            return updated ? Ok(new { message = "Session updated." }) : NotFound(new ApiException(404, "Session not found."));
        }

        // DELETE api/programs/sessions/{sessionId}  — delete a session
        [HttpDelete("sessions/{sessionId:int}")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> DeleteSession(int sessionId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var deleted = await _programService.DeleteSessionAsync(sessionId, coachId.Value);
            return deleted ? Ok(new { message = "Session deleted." }) : NotFound(new ApiException(404, "Session not found."));
        }

        // POST api/programs/{id}/publish  — coach publishes immediately
        [HttpPost("{id:int}/publish")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Publish(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            await _programService.PublishProgramAsync(id, coachId.Value);
            return Ok(new { message = "Program published and visible to trainees." });
        }

        // POST api/programs/{id}/unpublish  — coach hides from catalogue
        [HttpPost("{id:int}/unpublish")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Unpublish(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            await _programService.UnpublishProgramAsync(id, coachId.Value);
            return Ok(new { message = "Program unpublished. Enrolled trainees are unaffected." });
        }

        // DELETE api/programs/{id}  — coach deletes their own program
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Delete(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            await _programService.DeleteProgramAsync(id, coachId.Value);
            return Ok(new { message = "Program deleted." });
        }

        // ── Admin ─────────

        // DELETE api/programs/admin/{id}  — admin can delete any program
        [HttpDelete("admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AdminDelete(int id)
        {
            var deleted = await _programService.AdminDeleteProgramAsync(id);
            return deleted ? Ok(new { message = "Program deleted by admin." }) : NotFound(new ApiException(404, "Program not found."));
        }

        // ── Helpers ───────

        private async Task<int?> ResolveCoachIdAsync()
        {
            var profile = await _coachService.GetMyProfileAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return profile?.Id;
        }

        private ActionResult CoachNotFound()
            => Unauthorized(new ApiException(401, "Coach profile not found."));
    }
}
