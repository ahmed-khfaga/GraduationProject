using System.ComponentModel.DataAnnotations.Schema;
using FitZone.Core.Comman;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    // ════════════════════════════════════════════════════════════════════════
    // NUTRITION PLAN
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A structured nutrition program created by a coach. Mirrors WorkoutProgram.
    ///
    /// CalorieStrategyType drives how each enrollee's target is derived:
    ///   Absolute     — BaselineCalories = AbsoluteCalorieTarget (same for everyone).
    ///   TDEERelative — BaselineCalories = enrollee's TDEE ± TDEEAdjustmentKcal
    ///                  so the target auto-scales to each individual's body weight.
    ///
    /// ProteinTargetPerKg is stored as a ratio (e.g. 2.2 = 2.2 g/kg body weight)
    /// so one plan serves trainees of different sizes without recalculation.
    ///
    /// CoachID is NEVER accepted from the request body — always injected from JWT.
    /// </summary>
    public class NutritionPlan : BaseEntity
    {
        [ForeignKey("CoachID")]
        public int CoachID { get; set; }

        /// <summary>
        /// Optional. When set this plan is designed to pair with the specified
        /// WorkoutProgram. Enrolment as a bundle syncs both week unlock engines.
        /// </summary>
        public int? LinkedWorkoutProgramID { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string? ExpectedOutcome { get; set; }
        public string? NextSteps { get; set; }

        public TrainingGoal  TrainingGoal  { get; set; }
        public FitnessLevel  FitnessLevel  { get; set; }
        public EquipmentType EquipmentType { get; set; }

        public int DurationOnWeeks { get; set; }

        public CalorieStrategyType CalorieStrategyType { get; set; }

        /// <summary>
        /// Used when CalorieStrategyType = TDEERelative.
        /// Positive = surplus (e.g. 250 means TDEE + 250).
        /// Negative = deficit (e.g. -400 means TDEE - 400).
        /// The service stores the signed value as written; UI shows the absolute with direction.
        /// </summary>
        public int? TDEEAdjustmentKcal { get; set; }

        /// <summary>Used when CalorieStrategyType = Absolute.</summary>
        public int? AbsoluteCalorieTarget { get; set; }

        /// <summary>
        /// Protein target as g per kg of body weight.
        /// EnrollmentService uses the trainee's current weight to compute absolute grams.
        /// Default 2.0 g/kg works for most goals; use 2.2 for fat loss to protect lean mass.
        /// </summary>
        public decimal ProteinTargetPerKg { get; set; } = 2.0m;

        public bool      IsPublished      { get; set; } = false;
        public DateTime? PublishedAt      { get; set; }
        public string?   PhotoThumbnailUrl { get; set; }

        public virtual Coach             Coach                    { get; set; }
        public virtual WorkoutProgram?   LinkedWorkoutProgram     { get; set; }

        public virtual ICollection<NutritionWeek>              NutritionWeeks             { get; set; } = new HashSet<NutritionWeek>();
        public virtual ICollection<TraineeNutritionEnrollment> TraineeNutritionEnrollments { get; set; } = new HashSet<TraineeNutritionEnrollment>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // NUTRITION WEEK
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// One week inside a NutritionPlan. Mirrors ProgramWeek.
    ///
    /// CalorieModifier is a signed decimal fraction applied to the enrollee's baseline:
    ///   +0.08 = +8 % (high-volume week)
    ///   -0.10 = -10 % (deload week)
    ///    0.00 = no change (standard week)
    ///
    /// The coaching narrative fields (ProgressionNote, FocusNote, NextWeekPreview)
    /// mirror those on ProgramWeek — coaches should always explain the WHY.
    /// </summary>
    public class NutritionWeek : BaseEntity
    {
        [ForeignKey("NutritionPlanID")]
        public int NutritionPlanID { get; set; }

        public int              WeekNumber       { get; set; }
        public WeekProtocolType WeekProtocolType { get; set; } = WeekProtocolType.Standard;

        /// <summary>
        /// Percentage modifier on the enrollee's baseline calorie target for this week.
        /// Stored as decimal fraction: 0.08 = +8 %, -0.10 = −10 %.
        /// </summary>
        public decimal CalorieModifier { get; set; } = 0m;

        public string? WeekDescription  { get; set; }
        public string? FocusNote        { get; set; }
        public string? ProgressionNote  { get; set; }
        public string? NextWeekPreview  { get; set; }

        public virtual NutritionPlan              NutritionPlan { get; set; }
        public virtual ICollection<DayProtocol>   DayProtocols  { get; set; } = new HashSet<DayProtocol>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // DAY PROTOCOL
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A typed nutrition protocol for one day inside a NutritionWeek. Mirrors WorkoutSession.
    ///
    /// DayProtocolType determines the macro template:
    ///   TrainingDay = higher carbs, lower fat.
    ///   RestDay     = lower carbs, higher fat, same protein.
    ///   HighDay     = carbs elevated to maintenance; used as a refeed.
    ///   DeloadDay   = moderate carbs, reduced overall volume.
    ///
    /// LinkedWorkoutSessionID couples this day's nutrition to a specific training session.
    /// When set, the PostWorkout/PreWorkout meal timing fields are relative to that session.
    /// </summary>
    public class DayProtocol : BaseEntity
    {
        [ForeignKey("NutritionWeekID")]
        public int NutritionWeekID { get; set; }

        public DayProtocolType DayProtocolType { get; set; }
        public WeekDay         WeekDay         { get; set; }

        /// <summary>
        /// When the same DayProtocolType appears more than once on the same day (rare),
        /// DayOrder controls display sequence (1 = first, 2 = second).
        /// </summary>
        public int DayOrder { get; set; } = 1;

        /// <summary>
        /// Optional. Links to the WorkoutSession happening on this day so meal timing
        /// (PreWorkout −120 min, PostWorkout +30 min) can reference the session start time.
        /// </summary>
        public int? LinkedWorkoutSessionID { get; set; }

        public int TotalCaloriesTarget { get; set; }
        public int ProteinTargetG      { get; set; }
        public int CarbTargetG         { get; set; }
        public int FatTargetG          { get; set; }

        public string? ProtocolNotes { get; set; }

        public virtual NutritionWeek       NutritionWeek         { get; set; }
        public virtual WorkoutSession?     LinkedWorkoutSession  { get; set; }
        public virtual ICollection<Meal>   Meals                 { get; set; } = new HashSet<Meal>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // MEAL
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// One eating occasion within a DayProtocol. Mirrors SessionExercise (the assignment layer).
    ///
    /// MealOrder controls display sequence within the day.
    ///
    /// TimeFromTrainingMinutes is only meaningful for PreWorkout and PostWorkout:
    ///   Negative = eat N minutes BEFORE training starts   (e.g. -120 = 2 h before)
    ///   Positive = eat within N minutes AFTER training    (e.g. +30  = within 30 min post)
    /// </summary>
    public class Meal : BaseEntity
    {
        [ForeignKey("DayProtocolID")]
        public int DayProtocolID { get; set; }

        public string         Name       { get; set; }
        public MealTimingType TimingType { get; set; }
        public int            MealOrder  { get; set; } = 1;

        /// <summary>
        /// Only meaningful for PreWorkout / PostWorkout timing types.
        /// Negative = minutes before training. Positive = minutes after.
        /// </summary>
        public int? TimeFromTrainingMinutes { get; set; }

        public int TargetCalories { get; set; }
        public int TargetProteinG { get; set; }
        public int TargetCarbG    { get; set; }
        public int TargetFatG     { get; set; }

        public string? Notes { get; set; }

        public virtual DayProtocol              DayProtocol   { get; set; }
        public virtual ICollection<MealFoodItem> MealFoodItems { get; set; } = new HashSet<MealFoodItem>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // MEAL FOOD ITEM  (join entity)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Assigns a FoodItem to a Meal with a specific amount and optional swap group.
    /// Mirrors SessionExercise (the assignment of an Exercise to a WorkoutSession).
    ///
    /// SwapGroupID groups interchangeable food items within the same Meal.
    /// All items sharing the same SwapGroupID are alternatives — the trainee picks exactly one.
    /// null = standalone item with no swap option.
    ///
    /// IsOptional marks items that can be omitted without materially disrupting macro targets.
    /// </summary>
    public class MealFoodItem : BaseEntity
    {
        [ForeignKey("MealID")]
        public int MealID { get; set; }

        [ForeignKey("FoodItemID")]
        public int FoodItemID { get; set; }

        public decimal AmountGrams { get; set; }
        public bool    IsOptional  { get; set; } = false;

        /// <summary>
        /// Groups alternative food items. Trainee selects one per group.
        /// e.g. "breakfast carb source" = oats OR bread OR sweet potato (all same group).
        /// </summary>
        public int? SwapGroupID { get; set; }

        public virtual Meal     Meal     { get; set; }
        public virtual FoodItem FoodItem { get; set; }
    }

    // ════════════════════════════════════════════════════════════════════════
    // FOOD ITEM
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A food in the library. Follows the exact same Global/Private model as Exercise:
    ///   CoachID = null → global (seeded by admin; visible to all coaches; read-only).
    ///   CoachID set   → private to that coach only; full CRUD available to that coach.
    ///
    /// Macros are stored per 100 g. ServingSizeG + ServingSizeName provide a human-readable
    /// unit for display (e.g. ServingSizeG = 200, ServingSizeName = "1 medium chicken breast").
    ///
    /// OnDelete(SetNull) on CoachID — same as Exercise:
    ///   deleting a coach sets CoachID to null, preserving food data and making it global.
    /// </summary>
    public class FoodItem : BaseEntity
    {
        public int? CoachID { get; set; }

        public string       Name     { get; set; }
        public FoodCategory Category { get; set; }

        public decimal CaloriesPer100g { get; set; }
        public decimal ProteinPer100g  { get; set; }
        public decimal CarbPer100g     { get; set; }
        public decimal FatPer100g      { get; set; }
        public decimal FiberPer100g    { get; set; }

        /// <summary>Standard serving size in grams (for display).</summary>
        public int    ServingSizeG    { get; set; } = 100;

        /// <summary>Human-readable serving label, e.g. "1 cup cooked", "1 medium piece".</summary>
        public string ServingSizeName { get; set; } = "100g";

        /// <summary>true = minimally processed whole food; false = processed/packaged.</summary>
        public bool IsWhole { get; set; } = true;

        public virtual Coach?                     Coach        { get; set; }
        public virtual ICollection<MealFoodItem>  MealFoodItems { get; set; } = new HashSet<MealFoodItem>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // TRAINEE NUTRITION ENROLLMENT
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The live record linking a Trainee to a NutritionPlan. Mirrors TraineeProgramEnrollment.
    ///
    /// Week unlock uses the same Monday-anchored 7-day formula as training, PLUS
    /// an additional gate: the coach must have reviewed and approved the previous week's
    /// check-in before the next week unlocks (CoachApprovedAt must be set on WeeklyCheckIn).
    /// If no check-in was submitted, the coach manually approves the week unlock.
    ///
    /// BaselineCalories is set at enrollment from TDEE calculation and NEVER changes.
    /// CurrentAdjustedKcal starts equal to BaselineCalories and drifts week-by-week
    /// as the coach approves adjustments. The cumulative drift is tracked against
    /// ClientNutritionConstraints.MaxCumulativeDriftKcal.
    ///
    /// LinkedWorkoutEnrollmentID: when enrolled as a bundle, both week unlock engines run
    /// off the same StartDate and the nutrition week can never exceed the training week.
    /// </summary>
    public class TraineeNutritionEnrollment : BaseEntity
    {
        public int TraineeID       { get; set; }
        public int NutritionPlanID { get; set; }

        /// <summary>Set when enrolled as a bundle with a WorkoutProgram.</summary>
        public int? LinkedWorkoutEnrollmentID { get; set; }

        public DateTime  StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate   { get; set; }

        public NutritionEnrollmentStatus Status   { get; set; } = NutritionEnrollmentStatus.Active;
        public bool                      IsActive { get; set; } = true;

        /// <summary>
        /// Highest nutrition week the trainee can currently access.
        /// Advances only after (a) 7 days have elapsed AND (b) the coach has approved
        /// the previous week's check-in (WeeklyCheckIn.CoachApprovedAt is set).
        /// </summary>
        public int MaxWeekUnlocked { get; set; } = 1;

        /// <summary>
        /// TDEE-derived calorie target computed at enrollment. Immutable after creation.
        /// Used as the reference point for cumulative drift tracking.
        /// </summary>
        public int BaselineCalories { get; set; }

        /// <summary>
        /// Live calorie target. Updated when coach approves a check-in adjustment.
        /// Starts equal to BaselineCalories; drifts within MaxCumulativeDriftKcal.
        /// </summary>
        public int CurrentAdjustedKcal { get; set; }

        /// <summary>
        /// Empirical TDEE computed by TDEEService after 4 weeks of real weight-change data.
        /// Surfaced to the coach for optional baseline recalibration review.
        /// null until sufficient data exists.
        /// </summary>
        public int? EmpiricalTDEEKcal { get; set; }

        public virtual Trainee                     Trainee                  { get; set; }
        public virtual NutritionPlan               NutritionPlan            { get; set; }
        public virtual TraineeProgramEnrollment?   LinkedWorkoutEnrollment  { get; set; }
        public virtual ClientNutritionConstraints? Constraints              { get; set; }

        public virtual ICollection<WeeklyCheckIn>  WeeklyCheckIns { get; set; } = new HashSet<WeeklyCheckIn>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // CLIENT NUTRITION CONSTRAINTS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Per-client constraint settings set by the coach for one specific enrollment.
    /// One-to-one with TraineeNutritionEnrollment.
    ///
    /// The ProposalEngine operates strictly within these bounds — it can never propose
    /// an adjustment that violates a floor, ceiling, or single-week cap.
    /// The coach sets these at enrollment and can update at any time.
    ///
    /// ExpectedWeeklyChangeMin/Max represent the acceptable weekly weight-delta range:
    ///   Fat loss  : both negative  e.g. Min = -0.60, Max = -0.40 (losing 400–600 g/wk)
    ///   Muscle gain: both positive e.g. Min = +0.10, Max = +0.25 (gaining 100–250 g/wk)
    ///   Maintenance: near zero     e.g. Min = -0.10, Max = +0.10
    /// </summary>
    public class ClientNutritionConstraints : BaseEntity
    {
        [ForeignKey("EnrollmentID")]
        public int EnrollmentID { get; set; }

        // ── Measurement ────────────────────────────────────────────────────

        /// <summary>How many consecutive morning weights to average (3, 5, or 7).</summary>
        public int WeightAveragingDays { get; set; } = 3;

        /// <summary>Lower bound of acceptable weekly weight-delta (kg). See class summary.</summary>
        public decimal ExpectedWeeklyChangeMin { get; set; }

        /// <summary>Upper bound of acceptable weekly weight-delta (kg). See class summary.</summary>
        public decimal ExpectedWeeklyChangeMax { get; set; }

        /// <summary>
        /// How far outside the expected band before the algorithm proposes a change (kg).
        /// e.g. 0.15 means the actual delta must be 0.15 kg outside the range before acting.
        /// Prevents over-reacting to minor weekly fluctuations.
        /// </summary>
        public decimal DeviationTriggerKg { get; set; } = 0.15m;

        // ── Absolute floors & ceilings (HARD LIMITS — algorithm never crosses) ──

        public int ProteinFloorG  { get; set; } = 160;
        public int FatFloorG      { get; set; } = 50;
        public int CalorieFloor   { get; set; } = 1600;
        public int CalorieCeiling { get; set; } = 5000;

        // ── Adjustment mechanics ───────────────────────────────────────────

        /// <summary>Maximum kcal the algorithm can propose in a single week (positive int).</summary>
        public int MaxSingleAdjustmentKcal { get; set; } = 150;

        /// <summary>
        /// Maximum total cumulative drift from BaselineCalories across all weeks.
        /// Once reached, the algorithm escalates to the coach for a baseline review
        /// rather than continuing to drift further from the original plan.
        /// </summary>
        public int MaxCumulativeDriftKcal { get; set; } = 500;

        /// <summary>Which macro absorbs the adjustment first. See AdjustmentVector enum.</summary>
        public AdjustmentVector PreferredAdjustmentVector { get; set; } = AdjustmentVector.RestDayCarbs;

        /// <summary>
        /// If self-reported adherence is below this %, the check-in data is flagged as
        /// unreliable and the algorithm returns a Low-confidence Deferred proposal.
        /// Coach still reviews but the system does not suggest a calorie change.
        /// </summary>
        public int AdherenceThresholdPercent { get; set; } = 75;

        // ── Special rule flags ─────────────────────────────────────────────

        /// <summary>
        /// When true, require 2 consecutive weeks of deviation before proposing adjustment.
        /// Reduces false positives for clients with naturally high weight variability.
        /// </summary>
        public bool RequireConsecutiveWeeksDeviation { get; set; } = false;

        /// <summary>
        /// When true, the algorithm flags high-volume training weeks as scale-noise weeks.
        /// Muscle inflammation causes water retention that masks fat loss on the scale.
        /// The deviation trigger is tightened automatically on these weeks.
        /// </summary>
        public bool ApplyTrainingWeekNoiseCorrection { get; set; } = true;

        /// <summary>
        /// When true, if the client reports EnergyLevel = 1 for any 2 consecutive weeks,
        /// the enrollment is escalated to the coach regardless of weight data.
        /// Catches under-fuelling before the scale data shows a problem.
        /// </summary>
        public bool EnergyLevelEscalationRule { get; set; } = true;

        /// <summary>
        /// When true, the algorithm uses smaller incremental adjustments and is less
        /// willing to deepen a deficit, prioritising lean-mass retention over speed.
        /// </summary>
        public bool PreserveLeanMassOverRate { get; set; } = false;

        /// <summary>
        /// When true, TDEEService computes an empirical TDEE from real weight-change data
        /// after week 4 and surfaces it to the coach for optional baseline recalibration.
        /// </summary>
        public bool EnableBaselineRecalibrationReview { get; set; } = true;

        public virtual TraineeNutritionEnrollment Enrollment { get; set; }
    }

    // ════════════════════════════════════════════════════════════════════════
    // WEEKLY CHECK-IN
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Weekly check-in submitted by the trainee. The most important record in the nutrition system.
    ///
    /// DATA SEPARATION — by design:
    ///   The five objective fields (average weight, energy, hunger, sleep, adherence) are
    ///   the ONLY inputs to the ProposalEngine algorithm.
    ///   ClientNote and NoteCategory are visible to the coach but are NEVER read by the algorithm.
    ///   This is not a technical limitation — it is an architectural decision.
    ///   The algorithm is goal-driven, not empathy-driven. The coach is the empathy layer.
    ///
    /// UNLOCK GATE:
    ///   WeekN+1 unlocks ONLY after CoachApprovedAt is set on this week's check-in.
    ///   If the client does not submit a check-in, the coach manually sets CoachApprovedAt
    ///   to release the next week. Everything is a deliberate coach decision.
    ///
    /// COACH NOTE:
    ///   CoachNote (max 200 chars) is a professional directive, not a conversation.
    ///   It appears at the top of the client's next week view. Read-only to the client.
    /// </summary>
    public class WeeklyCheckIn : BaseEntity
    {
        [ForeignKey("EnrollmentID")]
        public int EnrollmentID { get; set; }

        public int      WeekNumber  { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // ── Objective data — READ BY ALGORITHM ──────────────────────────────
        // Three consecutive morning weights are averaged to smooth daily fluctuation.
        // Weight2 and Weight3 are nullable because a coach can manually approve
        // a check-in for a client who only submitted one reading.

        public decimal  MorningWeight1 { get; set; }
        public decimal? MorningWeight2 { get; set; }
        public decimal? MorningWeight3 { get; set; }

        /// <summary>
        /// Calculated by CheckInService from the submitted morning weights.
        /// Average of all non-null readings (1, 2, or 3 values).
        /// </summary>
        public decimal AverageWeight { get; set; }

        public int EnergyLevel    { get; set; }  // 1–5
        public int HungerLevel    { get; set; }  // 1–5
        public int SleepQuality   { get; set; }  // 1–5
        public int AdherencePercent { get; set; } // 0–100

        // ── Client note — NOT READ BY ALGORITHM ─────────────────────────────

        /// <summary>
        /// Optional client note. Maximum 400 characters.
        /// Visible to the coach during review. NEVER passed to ProposalEngine.
        /// Coach reads it and decides independently whether it warrants action.
        /// </summary>
        public string?      ClientNote   { get; set; }
        public NoteCategory? NoteCategory { get; set; }

        // ── System proposal — generated by ProposalEngine ────────────────────

        public int?    SystemProposalKcal           { get; set; }
        public string? SystemProposalReasoning       { get; set; }
        public string? ProjectedOutcomeIfNoAction    { get; set; }
        public ProposalConfidence SystemConfidence   { get; set; } = ProposalConfidence.Medium;

        // ── Coach decision — recorded after review ────────────────────────────

        public CoachDecisionType? CoachDecision           { get; set; }
        public int?               FinalAdjustmentKcal     { get; set; }
        public AdjustmentVector?  AppliedAdjustmentVector { get; set; }

        /// <summary>
        /// Coach's professional note to the client. Maximum 200 characters.
        /// Delivered as a directive when week N+1 content unlocks.
        /// The client cannot reply within the platform — one-way communication.
        /// </summary>
        public string?        CoachNote       { get; set; }
        public CoachNoteAction? CoachNoteAction { get; set; }

        public DateTime? CoachReviewedAt { get; set; }

        /// <summary>
        /// Set by the coach to release week N+1. This is the unlock gate.
        /// Must be set before NutritionEnrollmentService advances MaxWeekUnlocked.
        /// For missed check-ins, the coach sets this manually without a proposal.
        /// </summary>
        public DateTime? CoachApprovedAt { get; set; }

        public virtual TraineeNutritionEnrollment Enrollment { get; set; }
    }
}
