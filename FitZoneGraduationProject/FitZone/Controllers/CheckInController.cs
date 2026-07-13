using System.Security.Claims;
using FitZone.Core.Enums;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    [Authorize]
    public class CheckInController : BaseApiController
    {
        private readonly ICheckInService      _checkInService;
        private readonly ICoachReviewService  _reviewService;
        private readonly ITraineeService      _traineeService;
        private readonly ICoachService        _coachService;

        public CheckInController(
            ICheckInService     checkInService,
            ICoachReviewService reviewService,
            ITraineeService     traineeService,
            ICoachService       coachService)
        {
            _checkInService = checkInService;
            _reviewService  = reviewService;
            _traineeService = traineeService;
            _coachService   = coachService;
        }

        // ════════════════════════════════════════════════════════════════════
        // TRAINEE — submit check-in
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/checkin/{enrollmentId}
        /// Trainee submits their weekly check-in for the current MaxWeekUnlocked.
        ///
        /// DATA CONTRACT:
        ///   The five objective fields (morning weights, energy, hunger, sleep, adherence)
        ///   feed the ProposalEngine algorithm.
        ///   ClientNote and NoteCategory are stored and shown to the coach — NEVER to the algorithm.
        ///
        /// After submission the system generates a proposal and the coach must review
        /// before week N+1 becomes accessible.
        /// </summary>
        [HttpPost("{enrollmentId:int}")]
        public async Task<ActionResult<CheckInConfirmationDto>> Submit(
            int enrollmentId, [FromBody] SubmitCheckInDto dto)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var confirmation = await _checkInService.SubmitAsync(
                enrollmentId, traineeId.Value, dto);
            return Ok(confirmation);
        }
        // ── TRAINEE — check-in history / progress timeline ──────────────────────

        /// <summary>
        /// GET /api/checkin/{enrollmentId}/history
        /// Returns the trainee's own check-in history: weight per week, energy/hunger/sleep,
        /// calories in effect, any calorie adjustment, and the coach's directive note.
        /// Ordered oldest-to-newest for timeline/graph rendering.
        /// </summary>
        [HttpGet("{enrollmentId:int}/history")]
        public async Task<ActionResult<IEnumerable<TraineeCheckInHistoryDto>>> GetMyHistory(
            int enrollmentId)
        {
            var traineeId = await ResolveTraineeIdAsync();
            if (traineeId is null) return TraineeNotFound();

            var history = await _checkInService.GetTraineeHistoryAsync(
                enrollmentId, traineeId.Value);
            return Ok(history);
        }
        // ════════════════════════════════════════════════════════════════════
        // COACH — review queue
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/checkin/coach/queue
        /// Returns the coach's full weekly review queue — all active enrollments across
        /// all plans owned by this coach, sorted by priority:
        ///   Escalated → Proposal → OnTrack → NoCheckIn.
        ///
        /// This is the coach's Monday-morning inbox. Every client in this list
        /// needs a decision before their next week unlocks.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpGet("coach/queue")]
        public async Task<ActionResult<IEnumerable<CoachReviewQueueItemDto>>> GetReviewQueue()
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var queue = await _reviewService.GetReviewQueueAsync(coachId.Value);
            return Ok(queue);
        }

        /// <summary>
        /// GET /api/checkin/coach/{checkInId}
        /// Full check-in proposal detail for the coach review panel.
        /// Includes: weight graph history, all objective data, client note (separate section),
        /// system proposal with full reasoning, confidence score, and projected outcome.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpGet("coach/{checkInId:int}")]
        public async Task<ActionResult<CheckInProposalDto>> GetReviewDetail(int checkInId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var detail = await _reviewService.GetReviewDetailAsync(checkInId, coachId.Value);
            if (detail is null)
                return NotFound(new ApiException(404,
                    "Check-in not found or access denied."));

            return Ok(detail);
        }

        // ════════════════════════════════════════════════════════════════════
        // COACH — record decision
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/checkin/coach/{checkInId}/decide
        /// Coach records their decision on a pending check-in.
        ///
        /// This is the most important write operation in the nutrition system.
        /// It does four things atomically:
        ///   1. Records CoachDecision, FinalAdjustmentKcal, CoachNote, NoteAction.
        ///   2. Sets CoachApprovedAt — this releases the week unlock gate.
        ///   3. Updates enrollment.CurrentAdjustedKcal.
        ///   4. Propagates macro changes to week N+1 DayProtocols.
        ///
        /// Decision types:
        ///   Approved  — accepts system proposal as-is.
        ///   Modified  — accepts proposal but changes the kcal amount.
        ///   Override  — ignores proposal; coach enters custom adjustment.
        ///   Deferred  — no change this week; targets maintained for week N+1.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpPost("coach/{checkInId:int}/decide")]
        public async Task<ActionResult> Decide(
            int checkInId, [FromBody] CoachCheckInDecisionDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            await _checkInService.ApplyDecisionAsync(checkInId, coachId.Value, dto);

            return Ok(new
            {
                message = dto.Decision switch
                {
                    CoachDecisionType.Approved  => "Proposal approved. Week unlocked. Calorie adjustment applied.",
                    CoachDecisionType.Modified  => "Modified proposal saved. Week unlocked. Adjustment applied.",
                    CoachDecisionType.Override  => "Custom adjustment saved. Week unlocked.",
                    CoachDecisionType.Deferred  => "Decision deferred. Targets unchanged. Week unlocked.",
                    _                           => "Decision recorded."
                }
            });
        }

        /// <summary>
        /// POST /api/checkin/coach/enrollment/{enrollmentId}/manual-unlock
        /// Coach manually unlocks the next week when no check-in was submitted.
        /// Creates a placeholder check-in with CoachApprovedAt set, no adjustment applied.
        /// Use this when a trainee missed their check-in and you want to release their week.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpPost("coach/enrollment/{enrollmentId:int}/manual-unlock")]
        public async Task<ActionResult> ManualUnlock(int enrollmentId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            await _checkInService.ManuallyApproveUnlockAsync(enrollmentId, coachId.Value);

            return Ok(new
            {
                message = "Week manually unlocked. " +
                          "A placeholder check-in has been created with no calorie adjustment."
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<int?> ResolveTraineeIdAsync()
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _traineeService.GetProfileAsync(userId!);
            return profile?.Id;
        }

        private async Task<int?> ResolveCoachIdAsync()
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _coachService.GetMyProfileAsync(userId!);
            return profile?.Id;
        }

        private ActionResult TraineeNotFound()
            => Unauthorized(new ApiException(401, "Trainee profile not found."));

        private ActionResult CoachNotFound()
            => Unauthorized(new ApiException(401, "Coach profile not found."));
    }
}
