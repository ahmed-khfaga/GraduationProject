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
    [Authorize]
    public class EnrollmentController : BaseApiController
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ITraineeService _traineeService;

        public EnrollmentController(IEnrollmentService enrollmentService, ITraineeService traineeService)
        {
            _enrollmentService = enrollmentService;
            _traineeService = traineeService;
        }

        // GET api/enrollment
        // Active enrollments dashboard — week unlock is computed automatically.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments()
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(traineeId.Value);
            return Ok(enrollments);
        }

        // GET api/enrollment/history
        // All enrollments including completed and cancelled — lets the trainee see their full history
        // and re-enroll in a program they previously started (will resume from saved week).
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<EnrollmentHistoryDto>>> GetHistory()
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var history = await _enrollmentService.GetMyEnrollmentHistoryAsync(traineeId.Value);
            return Ok(history);
        }

        // GET api/enrollment/{enrollmentId}/weeks/{weekNumber}
        // Returns the sessions for a specific week.
        // Returns 403 if the week is not yet unlocked.
        [HttpGet("{enrollmentId:int}/weeks/{weekNumber:int}")]
        public async Task<ActionResult<WeekDetailDto>> GetWeek(int enrollmentId, int weekNumber)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var week = await _enrollmentService.GetWeekAsync(enrollmentId, weekNumber, traineeId.Value);
            if (week is null)
                return NotFound(new ApiException(404, "Week not found for this program."));

            return Ok(week);
        }

        // GET api/enrollment/sessions/{sessionId}
        // Full session with exercises grouped by section (Warmup → Primer → MainWork → Cooldown).
        // Returns 403 if the session belongs to a locked week.
        [HttpGet("sessions/{sessionId:int}")]
        public async Task<ActionResult<WorkoutSessionDto>> GetSession(int sessionId)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var session = await _enrollmentService.GetSessionDetailAsync(sessionId, traineeId.Value);
            if (session is null)
                return NotFound(new ApiException(404, "Session not found."));

            return Ok(session);
        }

        // POST api/enrollment/start
        // Enroll in a program. If the trainee has a previous enrollment in this same program
        // (even if cancelled), it will be RESUMED from the saved week — not restarted.
        // If they are in a different program on the same track, that program is suspended first.
        [HttpPost("start")]
        public async Task<ActionResult<EnrollmentDto>> StartProgram([FromBody] StartProgramDto dto)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var enrollment = await _enrollmentService.StartProgramAsync(traineeId.Value, dto);
            return Ok(enrollment);
        }

        // DELETE api/enrollment/{id}
        // Cancels the enrollment. Progress is preserved — re-enrolling later will resume it.
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Cancel(int id)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            await _enrollmentService.CancelEnrollmentAsync(id, traineeId.Value);
            return Ok(new { message = "Enrollment cancelled. Your progress has been saved — you can resume this program any time." });
        }

        // ── Private helpers ──────

        private async Task<int?> ResolveTraineeIdAsync()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appUserId is null) return null;

            var profile = await _traineeService.GetProfileAsync(appUserId);
            return profile?.Id;
        }

        private ActionResult TraineeNotFound()
            => NotFound(new ApiException(404, "Trainee profile not found."));
    }
}
