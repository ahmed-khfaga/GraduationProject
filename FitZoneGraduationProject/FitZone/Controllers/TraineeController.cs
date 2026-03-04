using FitZone.Service.DTOs.ProfileDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.APIs.Controllers
{
    [Authorize]
    public class TraineeController : BaseApiController
    {
        private readonly ITraineeService _traineeService;

        public TraineeController(ITraineeService traineeService)
        {
            _traineeService = traineeService;
        }

        // GET api/trainees/me
        [HttpGet("me")]
        public async Task<ActionResult<TraineeProfileDto>> GetMyProfile()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _traineeService.GetProfileAsync(appUserId);

            if (profile is null)
                return NotFound(new ApiException(404, "Trainee profile not found."));

            return Ok(profile);
        }

        // PUT api/trainees/me
        [HttpPut("me")]
        public async Task<ActionResult> UpdateMyProfile([FromBody] UpdateTraineeProfileDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var updated = await _traineeService.UpdateProfileAsync(appUserId, dto);

            if (!updated)
                return NotFound(new ApiException(404, "Trainee profile not found."));

            return Ok(new { message = "Profile updated." });
        }
    }
}
