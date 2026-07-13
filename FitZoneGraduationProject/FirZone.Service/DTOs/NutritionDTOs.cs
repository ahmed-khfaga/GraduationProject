using System.ComponentModel.DataAnnotations;
using FitZone.Core.Enums;
using FitZone.Service.Validation;
using FitZone.Core.Specifications.Params;
// ══════════════════════════════════════════════════════════════════════════════
// NUTRITION PLAN DTOs
// ══════════════════════════════════════════════════════════════════════════════
namespace FitZone.Service.DTOs.NutritionDTOs
{
    // ── Create / Update ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new nutrition plan shell. CoachID is injected from JWT — never sent in the body.
    /// Weeks, day protocols, and meals are added via separate endpoints after creation.
    /// </summary>
    public class CreateNutritionPlanDto
    {
        public int?   LinkedWorkoutProgramID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ExpectedOutcome { get; set; }
        public string? NextSteps       { get; set; }

        public TrainingGoal        TrainingGoal        { get; set; }
        public FitnessLevel        FitnessLevel        { get; set; }
        public EquipmentType       EquipmentType       { get; set; }
        public CalorieStrategyType CalorieStrategyType { get; set; }

        [Required]
        public int DurationOnWeeks { get; set; }

        /// <summary>
        /// Used when CalorieStrategyType = TDEERelative.
        /// Signed: negative = deficit (e.g. -400), positive = surplus (e.g. +250).
        /// </summary>
        public int? TDEEAdjustmentKcal    { get; set; }

        /// <summary>Used when CalorieStrategyType = Absolute.</summary>
        public int? AbsoluteCalorieTarget { get; set; }

        /// <summary>Protein in grams per kg body weight. Default 2.0.</summary>
        [Range(1.0, 4.0)]
        public decimal ProteinTargetPerKg { get; set; } = 2.0m;

        public string? PhotoThumbnailUrl { get; set; }
    }

    /// <summary>Same shape as CreateNutritionPlanDto — full replacement semantics.</summary>
    public class UpdateNutritionPlanDto : CreateNutritionPlanDto { }

    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>Catalogue card — used in paginated public listing.</summary>
    public class NutritionPlanCardDto
    {
        public int     Id                { get; set; }
        public string  Name              { get; set; }
        public string  Description       { get; set; }
        public string? ExpectedOutcome   { get; set; }
        public string  CoachName         { get; set; }
        public decimal? CoachRating      { get; set; }
        public string  TrainingGoal      { get; set; }   // enum as string
        public string  FitnessLevel      { get; set; }
        public string  EquipmentType     { get; set; }
        public int     DurationOnWeeks   { get; set; }
        public bool    IsPublished       { get; set; }
        public bool    IsLinkedToProgram { get; set; }
        public string? PhotoThumbnailUrl { get; set; }
    }

    /// <summary>Full detail page — extends card with weeks and macronutrient strategy.</summary>
    public class NutritionPlanDetailDto : NutritionPlanCardDto
    {
        public string?  NextSteps           { get; set; }
        public string   CalorieStrategy     { get; set; }   // enum as string
        public int?     TDEEAdjustmentKcal  { get; set; }
        public int?     AbsoluteCalorieTarget { get; set; }
        public decimal  ProteinTargetPerKg  { get; set; }
        public int?     LinkedWorkoutProgramID { get; set; }
        public List<NutritionWeekSummaryDto> NutritionWeeks { get; set; } = new();
    }

    // NutritionPlanFilterParams is defined in FitZone.Core.Specifications.Params

    // ══════════════════════════════════════════════════════════════════════════
    // NUTRITION WEEK DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Adds a week to a plan including all day protocols and meals in one call.</summary>
    public class CreateNutritionWeekDto
    {
        [Range(1, 520, ErrorMessage = "WeekNumber is required and must be 1 or greater. " +
                                  "Did you forget to include weekNumber in the request body?")]
        public int WeekNumber { get; set; }

        public WeekProtocolType WeekProtocolType { get; set; } = WeekProtocolType.Standard;

        /// <summary>
        /// Signed percentage applied to each enrollee's baseline calories.
        /// +0.08 = +8 % (high-volume week), -0.10 = -10 % (deload). 0 = no change.
        /// </summary>
        [Range(-0.5, 0.5)]
        public decimal CalorieModifier { get; set; } = 0m;

        public string? WeekDescription { get; set; }
        public string? FocusNote       { get; set; }
        public string? ProgressionNote { get; set; }
        public string? NextWeekPreview { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one day protocol is required.")]
        public List<CreateDayProtocolDto> DayProtocols { get; set; } = new();
    }

    /// <summary>Updates only the narrative/metadata fields. Day protocols are unaffected.</summary>
    public class UpdateNutritionWeekDto
    {
        public WeekProtocolType? WeekProtocolType { get; set; }
        public decimal?          CalorieModifier  { get; set; }
        public string?           WeekDescription  { get; set; }
        public string?           FocusNote        { get; set; }
        public string?           ProgressionNote  { get; set; }
        public string?           NextWeekPreview  { get; set; }
    }

    /// <summary>
    /// One week of a nutrition plan — shown in the plan detail/preview page.
    ///
    /// IMPORTANT FOR FRONTEND: This DTO does NOT include per-trainee macro targets
    /// (ProteinTargetG, CarbTargetG, FatTargetG). Those are calculated at enrollment
    /// time using the trainee's individual TDEE and the week's CalorieModifier.
    /// The actual per-day macro targets are returned by:
    ///   GET /api/nutritionenrollment/{enrollmentId}/weeks/{weekNumber}
    /// as DayProtocolDto fields inside NutritionWeekDetailDto.
    ///
    /// What this DTO DOES expose:
    ///   CalorieModifier — signed fraction (e.g. +0.08 = +8% refeed, -0.10 = -10% deload).
    ///   DayProtocolCount — how many day protocols this week contains.
    /// These are sufficient for the plan preview page to convey the week's structure.
    /// </summary>
    public class NutritionWeekSummaryDto
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public string WeekProtocolType  { get; set; }
        public decimal CalorieModifier  { get; set; }
        public string? WeekDescription  { get; set; }
        public string? FocusNote        { get; set; }
        public string? ProgressionNote  { get; set; }
        public string? NextWeekPreview  { get; set; }
        public int    DayProtocolCount  { get; set; }
    }
    public class NutritionWeekCoachDetailDto
    {
        public int Id { get; set; }
        public int NutritionPlanID { get; set; }
        public int WeekNumber { get; set; }
        public string WeekProtocolType { get; set; }
        public decimal CalorieModifier { get; set; }
        public string? WeekDescription { get; set; }
        public string? FocusNote { get; set; }
        public string? ProgressionNote { get; set; }
        public string? NextWeekPreview { get; set; }
        public List<DayProtocolDto> DayProtocols { get; set; } = new();
    }
    /// <summary>
    /// Full week detail returned to the trainee when accessing a nutrition week.
    /// MaxWeekUnlocked gate must pass AND coach must have approved before this is served.
    /// </summary>
    public class NutritionWeekDetailDto
    {
        public int     WeekNumber       { get; set; }
        public string  WeekProtocolType { get; set; }
        public string? WeekDescription  { get; set; }
        public string? FocusNote        { get; set; }
        public string? ProgressionNote  { get; set; }
        public string? NextWeekPreview  { get; set; }
        public bool    IsUnlocked       { get; set; }

        /// <summary>
        /// Coach's note from the previous week's check-in review.
        /// null on week 1 (no prior check-in).
        /// </summary>
        public string? CoachDirectiveNote { get; set; }

        public List<DayProtocolDto> DayProtocols { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DAY PROTOCOL DTOs
    // ══════════════════════════════════════════════════════════════════════════
    
    [MacroConsistency]
    public class CreateDayProtocolDto
    {
        public DayProtocolType DayProtocolType      { get; set; }
        public WeekDay         WeekDay              { get; set; }
        public int             DayOrder             { get; set; } = 1;
        public int?            LinkedWorkoutSessionID { get; set; }

        [Required]
        public int TotalCaloriesTarget { get; set; }

        [Required]
        public int ProteinTargetG { get; set; }

        [Required]
        public int CarbTargetG { get; set; }

        [Required]
        public int FatTargetG { get; set; }

        public string? ProtocolNotes { get; set; }

        [Required]
        public List<CreateMealDto> Meals { get; set; } = new();
    }

    [MacroConsistencyOnUpdate]
    public class UpdateDayProtocolDto
    {
        public int?    TotalCaloriesTarget { get; set; }
        public int?    ProteinTargetG      { get; set; }
        public int?    CarbTargetG         { get; set; }
        public int?    FatTargetG          { get; set; }
        public string? ProtocolNotes       { get; set; }
    }

    /// <summary>Full day protocol returned to the trainee within a NutritionWeekDetailDto.</summary>
    public class DayProtocolDto
    {
        public int    Id                      { get; set; }
        public string DayProtocolType         { get; set; }  // enum as string
        public string WeekDay                 { get; set; }  // enum as string
        public int    DayOrder                { get; set; }
        public int    TotalCaloriesTarget     { get; set; }
        public int    ProteinTargetG          { get; set; }
        public int    CarbTargetG             { get; set; }
        public int    FatTargetG              { get; set; }
        public string? ProtocolNotes          { get; set; }
        public int?   LinkedWorkoutSessionID  { get; set; }
        public List<MealDto> Meals            { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MEAL DTOs
    // ══════════════════════════════════════════════════════════════════════════

    public class CreateMealDto
    {
        [Required]
        public string         Name       { get; set; }
        public MealTimingType TimingType { get; set; }
        public int            MealOrder  { get; set; } = 1;

        /// <summary>
        /// PreWorkout: negative (e.g. -120 = 2 h before).
        /// PostWorkout: positive (e.g. 30 = within 30 min after).
        /// </summary>
        public int? TimeFromTrainingMinutes { get; set; }

        [Required]
        public int TargetCalories { get; set; }

        [Required]
        public int TargetProteinG { get; set; }

        [Required]
        public int TargetCarbG { get; set; }

        [Required]
        public int TargetFatG { get; set; }

        public string? Notes { get; set; }

        public List<CreateMealFoodItemDto> FoodItems { get; set; } = new();
    }

    public class UpdateMealDto
    {
        public string?         Name                    { get; set; }
        public MealTimingType? TimingType              { get; set; }
        public int?            MealOrder               { get; set; }
        public int?            TimeFromTrainingMinutes { get; set; }
        public int?            TargetCalories          { get; set; }
        public int?            TargetProteinG          { get; set; }
        public int?            TargetCarbG             { get; set; }
        public int?            TargetFatG              { get; set; }
        public string?         Notes                   { get; set; }
    }

    public class MealDto
    {
        public int     Id                      { get; set; }
        public string  Name                    { get; set; }
        public string  TimingType              { get; set; }  // enum as string
        public int     MealOrder               { get; set; }
        public int?    TimeFromTrainingMinutes  { get; set; }
        public int     TargetCalories           { get; set; }
        public int     TargetProteinG           { get; set; }
        public int     TargetCarbG              { get; set; }
        public int     TargetFatG               { get; set; }
        public string? Notes                    { get; set; }
        public List<MealFoodItemDto> FoodItems  { get; set; } = new();
    }

    // ── MealFoodItem ──────────────────────────────────────────────────────────

    public class CreateMealFoodItemDto
    {
        [Required]
        public int     FoodItemID  { get; set; }

        [Required]
        [Range(0.1, 5000)]
        public decimal AmountGrams { get; set; }

        public bool IsOptional { get; set; } = false;

        /// <summary>
        /// Assign multiple food items the same SwapGroupID to make them alternatives.
        /// Trainee picks exactly one from each group.
        /// </summary>
        public int? SwapGroupID { get; set; }
    }

    public class MealFoodItemDto
    {
        public int     Id           { get; set; }
        public int     FoodItemID   { get; set; }
        public string  FoodName     { get; set; }
        public string  Category     { get; set; }
        public decimal AmountGrams  { get; set; }
        public bool    IsOptional   { get; set; }
        public int?    SwapGroupID  { get; set; }

        // Calculated macros for this specific amount
        public int MacroCalories { get; set; }
        public int MacroProteinG { get; set; }
        public int MacroCarbG    { get; set; }
        public int MacroFatG     { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // MEAL FOOD ITEM CRUD DTOs
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Adds a single food item to an existing meal.</summary>
    public class AddMealFoodItemDto
    {
        [Required]
        public int FoodItemID { get; set; }

        [Required]
        [Range(0.1, 5000, ErrorMessage = "Amount must be between 0.1g and 5,000g.")]
        public decimal AmountGrams { get; set; }

        public bool IsOptional { get; set; } = false;

        /// <summary>Assigns this item to an alternatives group. null = standalone item.</summary>
        public int? SwapGroupID { get; set; }
    }

    /// <summary>Partial update for a MealFoodItem. Only non-null fields are changed.</summary>
    public class UpdateMealFoodItemDto
    {
        [Range(0.1, 5000, ErrorMessage = "Amount must be between 0.1g and 5,000g.")]
        public decimal? AmountGrams { get; set; }

        public bool? IsOptional { get; set; }
        public int? SwapGroupID { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // FOOD ITEM DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a private food item in the coach's library.
    /// CoachID is injected from JWT — never from the body.
    /// </summary>
    public class CreateFoodItemDto
    {
        [Required]
        public string       Name         { get; set; }
        public FoodCategory Category     { get; set; }

        [Required]
        [Range(0, 900)]
        public decimal CaloriesPer100g { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal ProteinPer100g  { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal CarbPer100g     { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal FatPer100g      { get; set; }

        [Range(0, 100)]
        public decimal FiberPer100g    { get; set; } = 0m;

        public int    ServingSizeG    { get; set; } = 100;
        public string ServingSizeName { get; set; } = "100g";
        public bool   IsWhole         { get; set; } = true;
    }

    /// <summary>
    /// Summary used in the exercise-picker list inside the coach's meal builder.
    /// Shown in paginated list responses.
    /// </summary>
    public class FoodItemSummaryDto
    {
        public int     Id             { get; set; }
        public string  Name           { get; set; }
        public string  Category       { get; set; }    // enum as string
        public decimal CaloriesPer100g { get; set; }
        public decimal ProteinPer100g  { get; set; }
        public decimal CarbPer100g     { get; set; }
        public decimal FatPer100g      { get; set; }
        public int     ServingSizeG    { get; set; }
        public string  ServingSizeName { get; set; }

        /// <summary>true = global (admin-seeded, read-only). false = coach-private.</summary>
        public bool IsGlobal { get; set; }
    }

    public class FoodItemDetailDto : FoodItemSummaryDto
    {
        public decimal FiberPer100g { get; set; }
        public bool    IsWhole      { get; set; }
    }


    // ══════════════════════════════════════════════════════════════════════════
    // NUTRITION ENROLLMENT DTOs
    // ══════════════════════════════════════════════════════════════════════════

    public class StartNutritionEnrollmentDto
    {
        [Required]
        public int NutritionPlanID { get; set; }

        /// <summary>
        /// Optional. Links the nutrition enrollment to an existing training enrollment
        /// (bundle mode). Both week unlocks will be coupled from this point.
        /// </summary>
        public int? LinkedWorkoutEnrollmentID { get; set; }
    }

    public class NutritionEnrollmentDto
    {
        public int      Id                    { get; set; }
        public int      NutritionPlanID       { get; set; }
        public string   PlanName              { get; set; }
        public string   CoachName             { get; set; }
        public int      MaxWeekUnlocked       { get; set; }
        public int      TotalWeeks            { get; set; }
        public string   Status                { get; set; }  // enum as string
        public DateTime StartDate             { get; set; }
        public DateTime? EndDate              { get; set; }
        public int      BaselineCalories      { get; set; }
        public int      CurrentAdjustedKcal   { get; set; }
        public bool     PendingCheckIn        { get; set; }  // true if this week's CI not yet submitted
        public bool     PendingCoachReview    { get; set; }  // true if CI submitted but coach not reviewed
    }

    public class NutritionEnrollmentHistoryDto : NutritionEnrollmentDto
    {
        public bool IsActive { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // WEEKLY CHECK-IN DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Submitted by the trainee to unlock the next week.
    ///
    /// DATA CONTRACT RULE:
    ///   MorningWeight1/2/3, EnergyLevel, HungerLevel, SleepQuality, and AdherencePercent
    ///   are the ONLY fields that feed the ProposalEngine algorithm.
    ///   ClientNote is transmitted to the coach and logged — never to the algorithm.
    ///   This separation is enforced in CheckInService.SubmitAsync().
    /// </summary>
    public class SubmitCheckInDto
    {
        [Required]
        [Range(20, 500)]
        public decimal MorningWeight1 { get; set; }

        [Range(20, 500)]
        public decimal? MorningWeight2 { get; set; }

        [Range(20, 500)]
        public decimal? MorningWeight3 { get; set; }

        [Required]
        [Range(1, 5)]
        public int EnergyLevel { get; set; }

        [Required]
        [Range(1, 5)]
        public int HungerLevel { get; set; }

        [Required]
        [Range(1, 5)]
        public int SleepQuality { get; set; }

        [Required]
        [Range(0, 100)]
        public int AdherencePercent { get; set; }

        // ── Client note — NOT read by algorithm ───────────────────────
        /// <summary>
        /// Optional note for the coach. Maximum 400 characters.
        /// Use only for genuine health, injury, or life-event information.
        /// The algorithm never reads this field.
        /// </summary>
        [MaxLength(400)]
        public string?      ClientNote   { get; set; }
        public NoteCategory? NoteCategory { get; set; }
    }

    /// <summary>
    /// The proposal the system generates for the coach after a check-in is submitted.
    /// Contains both the data picture and the recommended action.
    /// </summary>
    public class CheckInProposalDto
    {
        public int    CheckInId     { get; set; }
        public int    EnrollmentId  { get; set; }
        public string TraineeName   { get; set; }
        public int    WeekNumber    { get; set; }

        // Objective data summary
        public decimal AverageWeight   { get; set; }
        public decimal WeightDeltaKg   { get; set; }  // vs previous week
        public decimal ExpectedMin     { get; set; }
        public decimal ExpectedMax     { get; set; }
        public int     EnergyLevel     { get; set; }
        public int     HungerLevel     { get; set; }
        public int     SleepQuality    { get; set; }
        public int     AdherencePercent { get; set; }

        // Client note — shown to coach separately from algorithm section
        public string?      ClientNote   { get; set; }
        public string?      NoteCategory { get; set; }

        // Algorithm output
        public int?    SystemProposalKcal        { get; set; }
        public string  SystemProposalReasoning   { get; set; }
        public string  SystemConfidence           { get; set; }  // enum as string
        public string? ProjectedOutcomeIfNoAction { get; set; }

        // Historical context
        public List<WeightHistoryPointDto> WeightHistory { get; set; } = new();
    }

    public class WeightHistoryPointDto
    {
        public int     WeekNumber    { get; set; }
        public decimal AverageWeight { get; set; }
        public int?    CaloriesApplied { get; set; }
    }

    /// <summary>
    /// Coach submits this to record their decision on a pending check-in.
    /// This is what triggers the week N+1 unlock and applies the calorie adjustment.
    /// </summary>
    public class CoachCheckInDecisionDto
    {
        [Required]
        public CoachDecisionType Decision { get; set; }

        /// <summary>
        /// The calorie adjustment to apply. Required unless Decision = Deferred.
        /// May differ from SystemProposalKcal when Decision = Modified or Override.
        /// Must be 0 when Decision = Deferred (no change).
        /// </summary>
        public int? FinalAdjustmentKcal { get; set; }

        public AdjustmentVector? AppliedAdjustmentVector { get; set; }

        /// <summary>How the coach acted on the client's note.</summary>
        public CoachNoteAction? NoteAction { get; set; }

        /// <summary>
        /// Professional note to client. Maximum 200 characters.
        /// Delivered as a directive when week N+1 content unlocks.
        /// Optional but strongly encouraged — silence feels impersonal.
        /// </summary>
        [MaxLength(500)]
        public string? CoachNote { get; set; }
    }

    /// <summary>Returned to the trainee to confirm their check-in was received.</summary>
    public class CheckInConfirmationDto
    {
        public int      CheckInId     { get; set; }
        public int      WeekNumber    { get; set; }
        public decimal  AverageWeight { get; set; }
        public string   Message       { get; set; }
        public bool     CoachReviewPending { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CLIENT NUTRITION CONSTRAINTS DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Coach creates or updates per-client constraints for a specific enrollment.
    /// Defaults are provided by ConstraintService based on the plan's TrainingGoal.
    /// Coach can update at any time — changes take effect from the next proposal cycle.
    /// </summary>
    public class SetConstraintsDto
    {
        [Range(3, 7)]
        public int WeightAveragingDays { get; set; } = 3;

        [Range(-2.0, 2.0)]
        public decimal ExpectedWeeklyChangeMin { get; set; }

        [Range(-2.0, 2.0)]
        public decimal ExpectedWeeklyChangeMax { get; set; }

        [Range(0.05, 0.5)]
        public decimal DeviationTriggerKg { get; set; } = 0.15m;

        [Range(100, 400)]
        public int ProteinFloorG { get; set; } = 160;

        [Range(30, 150)]
        public int FatFloorG { get; set; } = 50;

        [Range(1200, 2500)]
        public int CalorieFloor { get; set; } = 1600;

        [Range(2000, 8000)]
        public int CalorieCeiling { get; set; } = 5000;

        [Range(50, 300)]
        public int MaxSingleAdjustmentKcal { get; set; } = 150;

        [Range(200, 1000)]
        public int MaxCumulativeDriftKcal { get; set; } = 500;

        public AdjustmentVector PreferredAdjustmentVector { get; set; } = AdjustmentVector.RestDayCarbs;

        [Range(50, 100)]
        public int AdherenceThresholdPercent { get; set; } = 75;

        public bool RequireConsecutiveWeeksDeviation    { get; set; } = false;
        public bool ApplyTrainingWeekNoiseCorrection    { get; set; } = true;
        public bool EnergyLevelEscalationRule           { get; set; } = true;
        public bool PreserveLeanMassOverRate            { get; set; } = false;
        public bool EnableBaselineRecalibrationReview   { get; set; } = true;
    }

    public class ConstraintsDto : SetConstraintsDto
    {
        public int Id           { get; set; }
        public int EnrollmentID { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TDEE DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returned to the client before enrollment so they can see projected targets.
    /// Also stored on the enrollment record as BaselineCalories.
    /// </summary>
    public class TDEEResultDto
    {
        public int     BMR                  { get; set; }
        public int     TDEE                 { get; set; }
        public int     AdjustedCalories     { get; set; }
        public int     ProteinTargetG       { get; set; }
        public int     CarbTargetG          { get; set; }
        public int     FatTargetG           { get; set; }
        public string  Goal                 { get; set; }
        public string  ActivityLevel        { get; set; }
        public string  CalculationMethod    { get; set; }  // "Mifflin-St Jeor"
    }
    // ══════════════════════════════════════════════════════════════════════
    // TRAINEE CHECK-IN HISTORY DTO
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// One row in the trainee's progress timeline.
    /// Returned by GET /api/checkin/{enrollmentId}/history.
    /// </summary>
    public class TraineeCheckInHistoryDto
    {
        public int WeekNumber { get; set; }
        public DateTime SubmittedAt { get; set; }

        /// <summary>null when coach created a manual-unlock placeholder (no real weight).</summary>
        public decimal? AverageWeight { get; set; }

        public int EnergyLevel { get; set; }
        public int HungerLevel { get; set; }
        public int SleepQuality { get; set; }
        public int AdherencePercent { get; set; }

        /// <summary>The calorie target that was in effect during this week.</summary>
        public int CaloriesApplied { get; set; }

        /// <summary>Signed kcal adjustment after coach review. null = pending or no change.</summary>
        public int? AdjustmentKcal { get; set; }

        /// <summary>Coach's written directive shown at the top of week N+1.</summary>
        public string? CoachDirectiveNote { get; set; }

        /// <summary>true = coach has reviewed; false = pending review.</summary>
        public bool CoachReviewed { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // COACH REVIEW QUEUE DTOs
    // ══════════════════════════════════════════════════════════════════════════

    public enum ReviewPriority
    {
        Escalated = 0,  // Energy escalation or 3+ weeks off track
        Proposal  = 1,  // System has a proposal waiting for coach decision
        OnTrack   = 2,  // On track — quick review recommended
        NoCheckIn = 3   // Trainee did not submit — coach decides whether to unlock
    }

    public class CoachReviewQueueItemDto
    {
        public int            EnrollmentId    { get; set; }
        public int            CheckInId       { get; set; }  // 0 if no check-in
        public string         TraineeName     { get; set; }
        public string         PlanName        { get; set; }
        public int            WeekNumber      { get; set; }
        public int            TotalWeeks      { get; set; }
        public ReviewPriority Priority        { get; set; }
        public string         PriorityLabel   { get; set; }  // enum as string

        // Quick stats for the list view
        public decimal? AverageWeight    { get; set; }
        public decimal? WeightDeltaKg    { get; set; }
        public int?     AdherencePercent { get; set; }
        public bool     HasClientNote    { get; set; }
        public string?  NoteCategory     { get; set; }

        // System proposal summary (shown inline in list)
        public int?    ProposedAdjustmentKcal { get; set; }
        public string? SystemConfidence        { get; set; }
    }
}
