using System.Security.Claims;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    [Authorize]
    public class NutritionEnrollmentController : BaseApiController
    {
        private readonly INutritionEnrollmentService _enrollmentService;
        private readonly ITraineeService             _traineeService;

        public NutritionEnrollmentController(
            INutritionEnrollmentService enrollmentService,
            ITraineeService             traineeService)
        {
            _enrollmentService = enrollmentService;
            _traineeService    = traineeService;
        }

        // ── Trainee dashboard ────────────────────────────────────────────────

        /// <summary>
        /// GET /api/nutritionenrollment
        /// All active nutrition enrollments for the logged-in trainee.
        /// MaxWeekUnlocked is synchronised on every call (dual-gate: time + coach approval).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NutritionEnrollmentDto>>> GetMyEnrollments()
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var result = await _enrollmentService.GetMyEnrollmentsAsync(traineeId.Value);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/nutritionenrollment/history
        /// All enrollments — active, completed, and cancelled — ordered by StartDate desc.
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<NutritionEnrollmentHistoryDto>>> GetHistory()
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var result = await _enrollmentService.GetMyEnrollmentHistoryAsync(traineeId.Value);
            return Ok(result);
        }

        // ── Week access ──────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/nutritionenrollment/{enrollmentId}/weeks/{weekNumber}
        /// Returns full week detail with day protocols, meals, and food items.
        /// Returns 404 when the week is locked (time gate or coach approval gate not passed).
        /// The client should never request locked weeks — use MaxWeekUnlocked to know the ceiling.
        /// </summary>
        [HttpGet("{enrollmentId:int}/weeks/{weekNumber:int}")]
        public async Task<ActionResult<NutritionWeekDetailDto>> GetWeek(
            int enrollmentId, int weekNumber)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var week = await _enrollmentService.GetWeekAsync(
                enrollmentId, weekNumber, traineeId.Value);

            // 404 covers: enrollment not found, wrong trainee, week locked, week does not exist.
            if (week is null)
                return NotFound(new ApiException(404,
                    "Week not found or not yet unlocked. " +
                    "Check MaxWeekUnlocked on your active enrollment."));

            return Ok(week);
        }

        // ── TDEE preview ─────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/nutritionenrollment/preview-tdee/{planId}
        /// Previews projected calorie and macro targets before enrolling.
        /// Requires the trainee to have their weight and height set on their profile.
        /// </summary>
        [HttpGet("preview-tdee/{planId:int}")]
        public async Task<ActionResult<TDEEResultDto>> PreviewTDEE(int planId)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var result = await _enrollmentService.PreviewTDEEAsync(traineeId.Value, planId);
            return Ok(result);
        }

        // ── Enrol or resume ──────────────────────────────────────────────────

        /// <summary>
        /// POST /api/nutritionenrollment/start
        /// Enrol in a nutrition plan or resume a previously cancelled one.
        /// If a prior cancelled enrollment exists for this plan, it is reactivated
        /// with StartDate back-dated so the time gate restores saved progress.
        /// Body: { nutritionPlanID: int, linkedWorkoutEnrollmentID?: int }
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<NutritionEnrollmentDto>> Start(
            [FromBody] StartNutritionEnrollmentDto dto)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var enrollment = await _enrollmentService.StartEnrollmentAsync(traineeId.Value, dto);
            return Ok(enrollment);
        }

        // ── Cancel ───────────────────────────────────────────────────────────

        /// <summary>
        /// DELETE /api/nutritionenrollment/{id}
        /// Cancel an active enrollment. Progress (MaxWeekUnlocked, check-in history,
        /// CurrentAdjustedKcal) is preserved — the trainee can resume at any time.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Cancel(int id)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            await _enrollmentService.CancelEnrollmentAsync(id, traineeId.Value);
            return Ok(new
            {
                message = "Nutrition enrollment cancelled. " +
                          "Your progress is saved — you can resume any time."
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<int?> ResolveTraineeIdAsync()
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _traineeService.GetProfileAsync(userId!);
            return profile?.Id;
        }

        private ActionResult TraineeNotFound()
            => Unauthorized(new ApiException(401, "Trainee profile not found."));
    }
}
