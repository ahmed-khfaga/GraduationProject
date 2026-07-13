using FitZone.Core.Specifications.Params;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


// ══════════════════════════════════════════════════════════════════════════════
// NUTRITION CONSTRAINT CONTROLLER
// ══════════════════════════════════════════════════════════════════════════════
namespace FitZone.APIs.Controllers
{
    /// <summary>
    /// Manages per-client constraint settings for a specific nutrition enrollment.
    /// Only the coach who owns the plan of the enrollment can read or update these.
    ///
    /// Constraints are the guardrails the ProposalEngine operates within:
    ///   floors, ceilings, adjustment caps, adherence thresholds, and special rules.
    ///
    /// Defaults are created automatically at enrollment time by ConstraintService.BuildDefaults().
    /// The coach refines them after observing 2–3 weeks of real data.
    /// Changes take effect from the next proposal cycle (next check-in submission).
    /// </summary>
    [Authorize(Roles = "Coach")]
    public class NutritionConstraintController : BaseApiController
    {
        private readonly IConstraintService _constraintService;
        private readonly ICoachService _coachService;

        public NutritionConstraintController(
            IConstraintService constraintService,
            ICoachService coachService)
        {
            _constraintService = constraintService;
            _coachService = coachService;
        }

        /// <summary>
        /// GET /api/nutritionconstraint/{enrollmentId}
        /// Returns the current constraints for a specific enrollment.
        /// The coach must own the nutrition plan of that enrollment.
        /// </summary>
        [HttpGet("{enrollmentId:int}")]
        public async Task<ActionResult<ConstraintsDto>> Get(int enrollmentId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var constraints = await _constraintService.GetConstraintsAsync(
                enrollmentId, coachId.Value);

            if (constraints is null)
                return NotFound(new ApiException(404,
                    "Constraints not found or you do not have access to this enrollment."));

            return Ok(constraints);
        }

        /// <summary>
        /// PUT /api/nutritionconstraint/{enrollmentId}
        /// Create or replace the constraints for a specific enrollment.
        ///
        /// This is a full-replacement PUT — send all fields.
        /// All values are validated against sensible ranges at the DTO level ([Range] attributes).
        /// The coach should ensure ExpectedWeeklyChangeMin &lt; ExpectedWeeklyChangeMax.
        ///
        /// Changes take effect from the NEXT check-in submission.
        /// The current week's proposal (if already generated) is not retroactively updated.
        /// </summary>
        [HttpPut("{enrollmentId:int}")]
        public async Task<ActionResult> Upsert(
            int enrollmentId, [FromBody] SetConstraintsDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            // Validate that min < max before saving.
            if (dto.ExpectedWeeklyChangeMin >= dto.ExpectedWeeklyChangeMax)
                return BadRequest(new ApiException(400,
                    "ExpectedWeeklyChangeMin must be strictly less than ExpectedWeeklyChangeMax. " +
                    $"Received: Min={dto.ExpectedWeeklyChangeMin}, Max={dto.ExpectedWeeklyChangeMax}."));

            // Validate that calorie floor < ceiling.
            if (dto.CalorieFloor >= dto.CalorieCeiling)
                return BadRequest(new ApiException(400,
                    "CalorieFloor must be strictly less than CalorieCeiling."));

            await _constraintService.UpsertConstraintsAsync(enrollmentId, coachId.Value, dto);

            return Ok(new
            {
                message = "Constraints updated. " +
                          "Changes will apply from the next check-in submission."
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<int?> ResolveCoachIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _coachService.GetMyProfileAsync(userId!);
            return profile?.Id;
        }

        private ActionResult CoachNotFound()
            => Unauthorized(new ApiException(401, "Coach profile not found."));
    }
}
