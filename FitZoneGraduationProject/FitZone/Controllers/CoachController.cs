using FitZone.Service.DTOs.ProfileDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.APIs.Controllers
{
    public class CoachController : BaseApiController
    {
        private readonly ICoachService _coachService;

        public CoachController(ICoachService coachService)
        {
            _coachService = coachService;
        }

        // GET api/coaches
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CoachProfileDto>>> GetAll()
        {
            var coaches = await _coachService.GetAllCoachesAsync();
            return Ok(coaches);
        }

        // GET api/coaches/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CoachProfileDto>> GetById(int id)
        {
            var coach = await _coachService.GetCoachByIdAsync(id);
            if (coach is null)
                return NotFound(new ApiException(404, "Coach not found."));

            return Ok(coach);
        }

        // GET api/coaches/me  — authenticated coach's own profile
        [HttpGet("me")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult<CoachProfileDto>> GetMyProfile()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var coach = await _coachService.GetMyProfileAsync(appUserId);

            if (coach is null)
                return NotFound(new ApiException(404, "Coach profile not found."));

            return Ok(coach);
        }

        // PUT api/coaches/me
        [HttpPut("me")]
        [Authorize(Roles = "Coach")]
        public async Task<ActionResult> UpdateMyProfile([FromBody] UpdateCoachProfileDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var updated = await _coachService.UpdateMyProfileAsync(appUserId, dto);

            if (!updated)
                return NotFound(new ApiException(404, "Coach profile not found."));

            return Ok(new { message = "Profile updated." });
        }
    }
}
