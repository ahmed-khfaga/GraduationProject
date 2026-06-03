using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
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

        // ── Dashboard ─────────────────────────────────────────────────────────

        /// GET api/enrollment
        /// Returns all active enrollments for the logged-in trainee.
        /// MaxWeekUnlocked is synced automatically so the progress bar is always current.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments()
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(traineeId.Value);
            return Ok(enrollments);
        }

        // ── History ───────────────────────────────────────────────────────────

        /// GET api/enrollment/history
        /// All enrollments including completed and cancelled.
        /// Lets the trainee see everything they have ever enrolled in and re-enroll
        /// from saved progress.
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<EnrollmentHistoryDto>>> GetHistory()
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var history = await _enrollmentService.GetMyEnrollmentHistoryAsync(traineeId.Value);
            return Ok(history);
        }

        // ── Week overview ───────────────────────────────────────────────

        [HttpGet("{enrollmentId:int}/weeks")]
        public async Task<ActionResult<IEnumerable<WeekOverviewDto>>> GetWeekOverview(int enrollmentId)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var weeks = await _enrollmentService.GetWeekOverviewAsync(enrollmentId, traineeId.Value);
            return Ok(weeks);
        }

        // ── Single week detail ────────────────────────────────────────────────

      
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

        // ── Session detail ────────────────────────────────────────────────────

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

        // ── Enroll ────────────────────────────────────────────────────────────

        [HttpPost("start")]
        public async Task<ActionResult<EnrollmentDto>> StartProgram([FromBody] StartProgramDto dto)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var enrollment = await _enrollmentService.StartProgramAsync(traineeId.Value, dto);
            return Ok(enrollment);
        }

        // ── Cancel ────────────────────────────────────────────────────────────

 
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Cancel(int id)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            await _enrollmentService.CancelEnrollmentAsync(id, traineeId.Value);
            return Ok(new { message = "Enrollment cancelled. Your progress has been saved — you can resume this program any time." });
        }

        // ── Private helpers ───────────────────────────────────────────────────

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
