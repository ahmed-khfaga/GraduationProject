using System.Security.Claims;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitZone.Core.Specifications.Params;
using FitZone.Service.DTOs.EnrollmentDTOs;

namespace FitZone.APIs.Controllers
{
    public class NutritionPlanController : BaseApiController
    {
        private readonly INutritionPlanService _planService;
        private readonly ICoachService         _coachService;

        public NutritionPlanController(
            INutritionPlanService planService,
            ICoachService         coachService)
        {
            _planService  = planService;
            _coachService = coachService;
        }

        // ── Public catalogue ─────────────────────────────────────────────────

        /// <summary>GET /api/nutritionplan — paginated published plans with optional filters.</summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<NutritionPlanCardDto>>> GetPublished(
            [FromQuery] NutritionPlanFilterParams filters)
        {
            var result = await _planService.GetPublishedPlansAsync(filters);
            return Ok(result);
        }

        /// <summary>GET /api/nutritionplan/{id} — full plan detail with all weeks.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<NutritionPlanDetailDto>> GetDetail(int id)
        {
            var plan = await _planService.GetPlanDetailAsync(id);
            if (plan is null) return NotFound(new ApiException(404, "Nutrition plan not found."));
            return Ok(plan);
        }

        // ── Coach — own plans ────────────────────────────────────────────────

        /// <summary>GET /api/nutritionplan/coach — all plans owned by the authenticated coach.</summary>
        [Authorize(Roles = "Coach")]
        [HttpGet("coach")]
        public async Task<ActionResult<IEnumerable<NutritionPlanCardDto>>> GetCoachPlans()
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            return Ok(await _planService.GetCoachPlansAsync(coachId.Value));
        }

        // ── Coach — plan CRUD ────────────────────────────────────────────────

        /// <summary>
        /// POST /api/nutritionplan — create a new nutrition plan shell.
        /// CoachID is injected from JWT — never from the body.
        /// Returns { id: N }.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] CreateNutritionPlanDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            int id = await _planService.CreatePlanAsync(coachId.Value, dto);
            return CreatedAtAction(nameof(GetDetail), new { id }, new { id });
        }

        /// <summary>PUT /api/nutritionplan/{id} — update plan metadata (full replacement).</summary>
        [Authorize(Roles = "Coach")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateNutritionPlanDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.UpdatePlanAsync(id, coachId.Value, dto);
            if (!ok) return NotFound(new ApiException(404, "Plan not found or access denied."));
            return Ok(new { message = "Nutrition plan updated." });
        }

        /// <summary>DELETE /api/nutritionplan/{id} — coach deletes own plan.</summary>
        [Authorize(Roles = "Coach")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.DeletePlanAsync(id, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Plan not found or access denied."));
            return Ok(new { message = "Nutrition plan deleted." });
        }

        /// <summary>DELETE /api/nutritionplan/admin/{id} — admin hard-deletes any plan.</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id:int}")]
        public async Task<ActionResult> AdminDelete(int id)
        {
            var ok = await _planService.AdminDeletePlanAsync(id);
            if (!ok) return NotFound(new ApiException(404, "Plan not found."));
            return Ok(new { message = "Nutrition plan deleted by admin." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/weeks/{weekId:int}")]
        public async Task<ActionResult<NutritionWeekCoachDetailDto>> AdminGetWeekDetail(int weekId)
        {
            var week = await _planService.AdminGetWeekDetailAsync(weekId);
            if (week is null) return NotFound(new ApiException(404, "Week not found."));
            return Ok(week);
        }

        // ── Coach — publish / unpublish ──────────────────────────────────────

        /// <summary>
        /// POST /api/nutritionplan/{id}/publish — publish to catalogue immediately.
        /// No admin approval needed. Sets IsPublished = true, PublishedAt = UtcNow.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpPost("{id:int}/publish")]
        public async Task<ActionResult> Publish(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.PublishPlanAsync(id, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Plan not found or access denied."));
            return Ok(new { message = "Nutrition plan published. Now visible in the catalogue." });
        }

        /// <summary>
        /// POST /api/nutritionplan/{id}/unpublish — hide from catalogue.
        /// Active enrollees are completely unaffected.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpPost("{id:int}/unpublish")]
        public async Task<ActionResult> Unpublish(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.UnpublishPlanAsync(id, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Plan not found or access denied."));
            return Ok(new { message = "Nutrition plan unpublished. Enrolled trainees are unaffected." });
        }

        // ── Coach — week management ──────────────────────────────────────────

        /// <summary>
        /// POST /api/nutritionplan/{id}/weeks — add a full week with all day protocols,
        /// meals, and food assignments in a single transaction.
        /// </summary>
        [Authorize(Roles = "Coach")]
        [HttpPost("{id:int}/weeks")]
        public async Task<ActionResult> AddWeek(int id, [FromBody] CreateNutritionWeekDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            await _planService.AddNutritionWeekAsync(id, coachId.Value, dto);
            return Ok(new { message = "Nutrition week added successfully." });
        }

        [Authorize(Roles = "Coach")]
        [HttpGet("weeks/{weekId:int}")]
        public async Task<ActionResult<NutritionWeekCoachDetailDto>> GetWeekDetail(int weekId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var week = await _planService.GetWeekDetailForCoachAsync(weekId, coachId.Value);
            if (week is null) return NotFound(new ApiException(404, "Week not found or access denied."));
            return Ok(week);
        }

        /// <summary>PUT /api/nutritionplan/weeks/{weekId} — update week narrative metadata only.</summary>
        [Authorize(Roles = "Coach")]
        [HttpPut("weeks/{weekId:int}")]
        public async Task<ActionResult> UpdateWeek(int weekId, [FromBody] UpdateNutritionWeekDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.UpdateNutritionWeekAsync(weekId, coachId.Value, dto);
            if (!ok) return NotFound(new ApiException(404, "Week not found or access denied."));
            return Ok(new { message = "Week updated." });
        }

        /// <summary>DELETE /api/nutritionplan/weeks/{weekId} — delete week and all its protocols/meals.</summary>
        [Authorize(Roles = "Coach")]
        [HttpDelete("weeks/{weekId:int}")]
        public async Task<ActionResult> DeleteWeek(int weekId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.DeleteNutritionWeekAsync(weekId, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Week not found or access denied."));
            return Ok(new { message = "Week and all its day protocols deleted." });
        }

        // ── Coach — day protocol management ──────────────────────────────────

        /// <summary>PUT /api/nutritionplan/protocols/{protocolId} — update protocol macro targets.</summary>
        [Authorize(Roles = "Coach")]
        [HttpPut("protocols/{protocolId:int}")]
        public async Task<ActionResult> UpdateProtocol(
            int protocolId, [FromBody] UpdateDayProtocolDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.UpdateDayProtocolAsync(protocolId, coachId.Value, dto);
            if (!ok) return NotFound(new ApiException(404, "Day protocol not found or access denied."));
            return Ok(new { message = "Day protocol updated." });
        }

        /// <summary>DELETE /api/nutritionplan/protocols/{protocolId} — delete protocol and its meals.</summary>
        [Authorize(Roles = "Coach")]
        [HttpDelete("protocols/{protocolId:int}")]
        public async Task<ActionResult> DeleteProtocol(int protocolId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.DeleteDayProtocolAsync(protocolId, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Day protocol not found or access denied."));
            return Ok(new { message = "Day protocol and its meals deleted." });
        }

        // ── Coach — meal management ───────────────────────────────────────────

        /// <summary>PUT /api/nutritionplan/meals/{mealId} — update meal metadata.</summary>
        [Authorize(Roles = "Coach")]
        [HttpPut("meals/{mealId:int}")]
        public async Task<ActionResult> UpdateMeal(int mealId, [FromBody] UpdateMealDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.UpdateMealAsync(mealId, coachId.Value, dto);
            if (!ok) return NotFound(new ApiException(404, "Meal not found or access denied."));
            return Ok(new { message = "Meal updated." });
        }

        /// <summary>DELETE /api/nutritionplan/meals/{mealId} — delete a meal.</summary>
        [Authorize(Roles = "Coach")]
        [HttpDelete("meals/{mealId:int}")]
        public async Task<ActionResult> DeleteMeal(int mealId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _planService.DeleteMealAsync(mealId, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Meal not found or access denied."));
            return Ok(new { message = "Meal deleted." });
        }
        // ── Coach — food item assignment management ──────────────────────────

        [Authorize(Roles = "Coach")]
        [HttpPost("meals/{mealId:int}/foods")]
        public async Task<ActionResult<object>> AddFoodToMeal(
            int mealId, [FromBody] AddMealFoodItemDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            int newId = await _planService.AddMealFoodItemAsync(mealId, coachId.Value, dto);
            return Ok(new { id = newId, message = "Food item added to meal." });
        }

        [Authorize(Roles = "Coach")]
        [HttpPut("foodassignments/{mealFoodItemId:int}")]
        public async Task<ActionResult> UpdateFoodAssignment(
            int mealFoodItemId, [FromBody] UpdateMealFoodItemDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var ok = await _planService.UpdateMealFoodItemAsync(mealFoodItemId, coachId.Value, dto);
            if (!ok) return NotFound(new ApiException(404, "Food assignment not found or access denied."));
            return Ok(new { message = "Food assignment updated." });
        }

        [Authorize(Roles = "Coach")]
        [HttpDelete("foodassignments/{mealFoodItemId:int}")]
        public async Task<ActionResult> DeleteFoodAssignment(int mealFoodItemId)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();
            var ok = await _planService.DeleteMealFoodItemAsync(mealFoodItemId, coachId.Value);
            if (!ok) return NotFound(new ApiException(404, "Food assignment not found or access denied."));
            return Ok(new { message = "Food item removed from meal." });
        }
        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<int?> ResolveCoachIdAsync()
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _coachService.GetMyProfileAsync(userId!);
            return profile?.Id;
        }

        private ActionResult CoachNotFound()
            => Unauthorized(new ApiException(401, "Coach profile not found."));
    }
}
