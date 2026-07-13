using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Core.Specifications.Params;
namespace FitZone.Service.Services.Contract
{
    // ══════════════════════════════════════════════════════════════════════════
    // ITDEEService
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pure calculation service — no database access.
    /// All methods are synchronous because they are arithmetic only.
    /// </summary>
    public interface ITDEEService
    {
        /// <summary>
        /// Calculates Basal Metabolic Rate using Mifflin-St Jeor.
        /// gender: "Male" | "Female" (case-insensitive).
        /// </summary>
        int CalculateBMR(string gender, decimal weightKg, decimal heightCm, int ageYears);

        /// <summary>
        /// Applies Harris-Benedict activity multiplier based on weekly training sessions.
        /// 0-1 sessions → 1.20 (sedentary), 2 → 1.375, 3-4 → 1.55, 5-6 → 1.725, 7+ → 1.90.
        /// </summary>
        int ApplyActivityMultiplier(int bmr, int sessionsPerWeek);

        /// <summary>
        /// Applies goal-specific calorie adjustment to TDEE.
        /// LoseFat → −400, BuildMuscle → +250, GetStronger → +50, etc.
        /// </summary>
        int ApplyGoalAdjustment(int tdee, TrainingGoal goal);

        /// <summary>
        /// Computes protein, carb, and fat gram targets from total adjusted calories and weight.
        /// Protein = ProteinTargetPerKg × weightKg.
        /// Fat = max(FatFloorG, 25% of calories ÷ 9).
        /// Carbs = remaining calories ÷ 4.
        /// </summary>
        (int proteinG, int carbG, int fatG) CalculateMacros(
            int     adjustedCalories,
            decimal weightKg,
            decimal proteinTargetPerKg,
            int     fatFloorG);

        /// <summary>
        /// Computes the full TDEE result DTO — combines all steps above.
        /// Called at enrollment to determine BaselineCalories.
        /// </summary>
        TDEEResultDto ComputeFullResult(
            string       gender,
            decimal      weightKg,
            decimal      heightCm,
            DateTime?    dateOfBirth,
            int          sessionsPerWeek,
            TrainingGoal goal,
            decimal      proteinTargetPerKg,
            int          fatFloorG,
            CalorieStrategyType strategyType,
            int?         absoluteTarget,
            int?         tdeeAdjustment);

        /// <summary>
        /// Computes empirical TDEE from actual weight-change data collected over N weeks.
        /// Used for the 4-week recalibration review.
        /// Formula: empiricalTDEE = actualCaloriesConsumed + (weightDeltaKg × 7700 ÷ weekCount).
        /// Returns null if insufficient data (fewer than 3 valid check-ins).
        /// </summary>
        int? ComputeEmpiricalTDEE(
            IReadOnlyList<WeeklyCheckIn> checkIns,
            int                         currentAdjustedKcal);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // IProposalEngine
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The algorithm. Reads ONLY objective check-in data and per-client constraints.
    /// NEVER reads ClientNote or NoteCategory — enforced by signature (no note parameter).
    /// Pure computation — no database writes.
    /// </summary>
    public interface IProposalEngine
    {
        /// <summary>
        /// Generates a calorie-adjustment proposal for the coach to review.
        ///
        /// Inputs:
        ///   currentCheckIn  — the check-in just submitted (objective data only).
        ///   allCheckIns     — all prior check-ins for this enrollment (for trend analysis).
        ///   constraints     — per-client bounds and rules set by the coach.
        ///   currentKcal     — the enrollment's CurrentAdjustedKcal at time of computation.
        ///   baselineKcal    — the enrollment's BaselineCalories (for drift tracking).
        ///   linkedPlanWeek  — optional; the WeekProtocolType of the linked training week
        ///                     (used for noise correction on high-volume weeks).
        ///
        /// Returns a ProposalResult with:
        ///   SuggestedAdjustmentKcal — signed (negative = reduce, positive = increase, 0 = no change).
        ///   Confidence              — High, Medium, or Low.
        ///   Reasoning               — structured text shown to the coach in the review panel.
        ///   ProjectedOutcome        — projected weight at program end if no action taken.
        ///   IsEscalated             — true if the energy escalation rule fired.
        /// </summary>
        ProposalResult GenerateProposal(
            WeeklyCheckIn                currentCheckIn,
            IReadOnlyList<WeeklyCheckIn> allCheckIns,
            ClientNutritionConstraints   constraints,
            int                          currentKcal,
            int                          baselineKcal,
            WeekProtocolType?            linkedPlanWeekType = null);
    }

    /// <summary>Output from ProposalEngine.GenerateProposal().</summary>
    public class ProposalResult
    {
        public int                SuggestedAdjustmentKcal  { get; init; }
        public int                NewCalorieTarget          { get; init; }
        public ProposalConfidence Confidence                { get; init; }
        public string             Reasoning                 { get; init; }
        public string?            ProjectedOutcome          { get; init; }
        public bool               IsEscalated               { get; init; }
        public bool               BaselineRecalibrationDue  { get; init; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // INutritionPlanService
    // ══════════════════════════════════════════════════════════════════════════

    public interface INutritionPlanService
    {
        // Public catalogue
        Task<PaginatedResult<NutritionPlanCardDto>> GetPublishedPlansAsync(NutritionPlanFilterParams filters);
        Task<NutritionPlanDetailDto?>               GetPlanDetailAsync(int planId);

        // Coach's own plans
        Task<IEnumerable<NutritionPlanCardDto>> GetCoachPlansAsync(int coachId);
        Task<NutritionWeekCoachDetailDto?> GetWeekDetailForCoachAsync(int weekId, int coachId);

        /// <summary>Admin variant of GetWeekDetailForCoachAsync — no ownership filter.</summary>
        Task<NutritionWeekCoachDetailDto?> AdminGetWeekDetailAsync(int weekId);
        // Plan CRUD (coach)
        Task<int>  CreatePlanAsync(int coachId, CreateNutritionPlanDto dto);
        Task<bool> UpdatePlanAsync(int planId, int coachId, UpdateNutritionPlanDto dto);
        Task<bool> DeletePlanAsync(int planId, int coachId);
        Task<bool> AdminDeletePlanAsync(int planId);

        // Publish / unpublish
        Task<bool> PublishPlanAsync(int planId, int coachId);
        Task<bool> UnpublishPlanAsync(int planId, int coachId);

        // Week management (coach)
        Task AddNutritionWeekAsync(int planId, int coachId, CreateNutritionWeekDto dto);
        Task<bool> UpdateNutritionWeekAsync(int weekId, int coachId, UpdateNutritionWeekDto dto);
        Task<bool> DeleteNutritionWeekAsync(int weekId, int coachId);

        // Day protocol management (coach)
        Task<bool> UpdateDayProtocolAsync(int dayProtocolId, int coachId, UpdateDayProtocolDto dto);
        Task<bool> DeleteDayProtocolAsync(int dayProtocolId, int coachId);

        // Meal management (coach)
        Task<bool> UpdateMealAsync(int mealId, int coachId, UpdateMealDto dto);
        Task<bool> DeleteMealAsync(int mealId, int coachId);
        // MealFoodItem granular management
        Task<int> AddMealFoodItemAsync(int mealId, int coachId, AddMealFoodItemDto dto);
        Task<bool> UpdateMealFoodItemAsync(int mealFoodItemId, int coachId, UpdateMealFoodItemDto dto);
        Task<bool> DeleteMealFoodItemAsync(int mealFoodItemId, int coachId);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // INutritionEnrollmentService
    // ══════════════════════════════════════════════════════════════════════════

    public interface INutritionEnrollmentService
    {
        // Trainee dashboard
        Task<IEnumerable<NutritionEnrollmentDto>>        GetMyEnrollmentsAsync(int traineeId);
        Task<IEnumerable<NutritionEnrollmentHistoryDto>> GetMyEnrollmentHistoryAsync(int traineeId);

        // Week access — gated by MaxWeekUnlocked + coach approval
        Task<NutritionWeekDetailDto?> GetWeekAsync(int enrollmentId, int weekNumber, int traineeId);

        // Enrol or resume
        Task<NutritionEnrollmentDto> StartEnrollmentAsync(int traineeId, StartNutritionEnrollmentDto dto);

        // Cancel — progress preserved
        Task CancelEnrollmentAsync(int enrollmentId, int traineeId);

        // TDEE preview — called before enrollment so trainee sees projected targets
        Task<TDEEResultDto> PreviewTDEEAsync(int traineeId, int planId);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ICheckInService
    // ══════════════════════════════════════════════════════════════════════════

    public interface ICheckInService
    {
        // Trainee submits check-in
        Task<CheckInConfirmationDto> SubmitAsync(int enrollmentId, int traineeId, SubmitCheckInDto dto);

        // Trainee views their own check-in history and weight trend
        Task<IEnumerable<TraineeCheckInHistoryDto>> GetTraineeHistoryAsync(
            int enrollmentId, int traineeId);

        // Coach reads the pending proposal for a check-in
        Task<CheckInProposalDto?> GetProposalAsync(int checkInId, int coachId);

        // Coach records their decision — triggers week N+1 unlock and applies adjustment
        Task ApplyDecisionAsync(int checkInId, int coachId, CoachCheckInDecisionDto dto);

        // Coach manually unlocks week without a check-in (missed check-in scenario)
        Task ManuallyApproveUnlockAsync(int enrollmentId, int coachId);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ICoachReviewService
    // ══════════════════════════════════════════════════════════════════════════

    public interface ICoachReviewService
    {
        /// <summary>
        /// Returns the coach's full weekly review queue — all active enrollments
        /// across all plans owned by this coach, sorted by priority:
        /// Escalated → Proposal → OnTrack → NoCheckIn.
        /// </summary>
        Task<IEnumerable<CoachReviewQueueItemDto>> GetReviewQueueAsync(int coachId);

        /// <summary>Full check-in proposal detail for the review panel.</summary>
        Task<CheckInProposalDto?> GetReviewDetailAsync(int checkInId, int coachId);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // IFoodItemService
    // ══════════════════════════════════════════════════════════════════════════

    public interface IFoodItemService
    {
        // Coach browses library (global + private)
        Task<PaginatedResult<FoodItemSummaryDto>> GetFoodItemsForCoachAsync(int coachId, FoodItemFilterParams filters);
        Task<FoodItemDetailDto?>                  GetFoodItemByIdForCoachAsync(int id, int coachId);

        // Coach creates private food item (CoachID injected from service, never from DTO)
        Task<int>  CreateFoodItemAsync(CreateFoodItemDto dto, int coachId);

        // Coach updates own private food item (global items → 404)
        Task<bool> UpdateFoodItemAsync(int id, CreateFoodItemDto dto, int coachId);

        // Coach deletes own private food item (global items → 404)
        Task<bool> DeleteFoodItemAsync(int id, int coachId);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // IConstraintService
    // ══════════════════════════════════════════════════════════════════════════

    public interface IConstraintService
    {
        /// <summary>
        /// Returns the constraints for a specific enrollment.
        /// Coach must own the plan of that enrollment.
        /// </summary>
        Task<ConstraintsDto?> GetConstraintsAsync(int enrollmentId, int coachId);


        Task UpsertConstraintsAsync(int enrollmentId, int coachId, SetConstraintsDto dto);
        /// <summary>
        /// Builds default constraints for a new enrollment based on plan goal.
        /// Called internally by NutritionEnrollmentService during StartEnrollmentAsync.
        /// </summary>
        SetConstraintsDto BuildDefaults(TrainingGoal goal, decimal weightKg);
    }
}
