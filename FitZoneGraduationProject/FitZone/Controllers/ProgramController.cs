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

        // GET api/programs  — public, all published programs with filtering
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ProgramCardDto>>> GetAll([FromQuery] ProgramFilterParams filters)
        {
            var result = await _programService.GetPublishedProgramsAsync(filters);
            return Ok(result);
        }

        // GET api/programs/{id}  — program detail page
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProgramDetailDto>> GetById(int id)
        {
            var program = await _programService.GetProgramDetailAsync(id);
            if (program is null)
                return NotFound(new ApiException(404, "Program not found."));

            return Ok(program);
        }

        // GET api/programs/coach/{coachId}  — public coach program list
        [HttpGet("coach/{coachId:int}")]
        public async Task<ActionResult<IEnumerable<ProgramCardDto>>> GetByCoach(int coachId)
        {
            var programs = await _programService.GetCoachProgramsAsync(coachId);
            return Ok(programs);
        }

        // GET api/programs/pending  — Admin only
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ProgramCardDto>>> GetPending()
        {
            var programs = await _programService.GetPendingProgramsAsync();
            return Ok(programs);
        }

        // POST api/programs  — Coach creates a draft
        [HttpPost]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Create([FromBody] CreateProgramDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var coach = await _coachService.GetMyProfileAsync(appUserId);

            if (coach is null)
                return Unauthorized(new ApiException(401, "Coach profile not found."));

            var programId = await _programService.CreateProgramAsync(coach.Id, dto);
            return CreatedAtAction(nameof(GetById), new { id = programId }, new { id = programId });
        }

        // POST api/programs/{id}/weeks  — Coach adds a week to their draft
        [HttpPost("{id:int}/weeks")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> AddWeek(int id, [FromBody] CreateProgramWeekDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var coach = await _coachService.GetMyProfileAsync(appUserId);

            if (coach is null)
                return Unauthorized(new ApiException(401, "Coach profile not found."));

            await _programService.AddProgramWeekAsync(id, coach.Id, dto);
            return Ok(new { message = $"Week {dto.WeekNumber} added successfully." });
        }

        // POST api/programs/{id}/submit  — Coach submits for admin review
        [HttpPost("{id:int}/submit")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> Submit(int id)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var coach = await _coachService.GetMyProfileAsync(appUserId);

            if (coach is null)
                return Unauthorized(new ApiException(401, "Coach profile not found."));

            await _programService.SubmitForReviewAsync(id, coach.Id);
            return Ok(new { message = "Program submitted for review." });
        }

        // POST api/programs/{id}/review  — Admin approves or rejects
        [HttpPost("{id:int}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Review(int id, [FromBody] AdminReviewDto dto)
        {
            await _programService.ReviewProgramAsync(id, dto);
            var action = dto.Approve ? "published" : "rejected";
            return Ok(new { message = $"Program {action}." });
        }
    }
}
