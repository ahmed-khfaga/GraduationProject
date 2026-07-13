using System.Security.Claims;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitZone.Core.Specifications.Params;

namespace FitZone.APIs.Controllers
{   /// <summary>
    /// Manages the food library for coaches.
    /// Follows the exact same pattern as ExerciseController — same Global/Private model,
    /// same pagination, same ownership rules, same disabled-controls-for-global convention.
    ///
    /// Global items (isGlobal = true): visible to all coaches, read-only.
    ///   Edit/delete return 400 with a clear message — disable these buttons in the UI.
    /// Private items (isGlobal = false): full CRUD for the owning coach only.
    ///
    /// CoachID is ALWAYS injected from the JWT — never from the request body.
    /// </summary>
    

    /// <summary>
    /// Manages the food item library for coaches.
    /// Follows the exact same Global/Private pattern as ExerciseController:
    ///   Global items  (CoachID == null) — visible to all coaches, read-only.
    ///   Private items (CoachID set)     — only the owning coach can CRUD them.
    ///
    /// CoachID is ALWAYS injected from the JWT — never accepted from the request body.
    /// </summary>
    [Authorize(Roles = "Coach")]
    public class FoodItemController : BaseApiController
    {
        private readonly IFoodItemService _foodItemService;
        private readonly ICoachService    _coachService;

        public FoodItemController(IFoodItemService foodItemService, ICoachService coachService)
        {
            _foodItemService = foodItemService;
            _coachService    = coachService;
        }

        // ── Read ─────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/fooditem
        /// Paginated list of all food items available to this coach:
        /// global items (CoachID == null) + their own private items (CoachID == coachId).
        /// Optional filters: category, name (partial match).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<FoodItemSummaryDto>>> GetAll(
            [FromQuery] FoodItemFilterParams filters)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var result = await _foodItemService.GetFoodItemsForCoachAsync(coachId.Value, filters);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/fooditem/{id}
        /// Full detail for a single food item — validates it is global or owned by this coach.
        /// Returns 404 for items owned by other coaches (access denied treated as not found).
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FoodItemDetailDto>> GetById(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var item = await _foodItemService.GetFoodItemByIdForCoachAsync(id, coachId.Value);
            if (item is null)
                return NotFound(new ApiException(404,
                    "Food item not found or it belongs to another coach."));

            return Ok(item);
        }

        // ── Create ───────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/fooditem
        /// Creates a new private food item in this coach's library.
        /// CoachID is injected from the JWT — never from the body.
        /// Returns { id: N }.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] CreateFoodItemDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            int id = await _foodItemService.CreateFoodItemAsync(dto, coachId.Value);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // ── Update ───────────────────────────────────────────────────────────

        /// <summary>
        /// PUT /api/fooditem/{id}
        /// Updates a coach-private food item.
        /// Returns 404 with a descriptive message if the item is global (read-only).
        /// Full replacement semantics — send all fields.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateFoodItemDto dto)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _foodItemService.UpdateFoodItemAsync(id, dto, coachId.Value);
            if (!ok)
                return NotFound(new ApiException(404,
                    "Food item not found or it cannot be edited " +
                    "(global food items are read-only)."));

            return Ok(new { message = "Food item updated." });
        }

        // ── Delete ───────────────────────────────────────────────────────────

        /// <summary>
        /// DELETE /api/fooditem/{id}
        /// Deletes a coach-private food item.
        /// Returns 404 with a descriptive message if the item is global (protected).
        /// Note: if this food item is currently assigned to a meal, the service will
        /// throw because of the NoAction FK constraint — remove meal assignments first.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var coachId = await ResolveCoachIdAsync();
            if (coachId is null) return CoachNotFound();

            var ok = await _foodItemService.DeleteFoodItemAsync(id, coachId.Value);
            if (!ok)
                return NotFound(new ApiException(404,
                    "Food item not found or it cannot be deleted " +
                    "(global food items are protected)."));

            return Ok(new { message = "Food item deleted." });
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
