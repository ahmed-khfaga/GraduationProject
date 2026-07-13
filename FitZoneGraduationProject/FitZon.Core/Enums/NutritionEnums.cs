namespace FitZone.Core.Enums
{
    // ── NutritionPlan ──────────────────────────────────────────────────────────

    /// <summary>
    /// Determines how the plan's baseline calorie target is derived for each enrollee.
    ///   Absolute    — coach sets a fixed number (same for every enrollee).
    ///   TDEERelative — coach sets a delta (e.g. −400 kcal); the system adds it to
    ///                  each enrollee's individually calculated TDEE so the target
    ///                  auto-scales to their body weight.
    /// </summary>
    public enum CalorieStrategyType
    {
        Absolute     = 0,
        TDEERelative = 1
    }

    /// <summary>Characterises the training load in a given nutrition week.</summary>
    public enum WeekProtocolType
    {
        Standard   = 0,
        HighVolume = 1,  // CalorieModifier typically +6 – +10 %
        Deload     = 2,  // CalorieModifier typically −8 – −12 %
        Refeed     = 3,  // Carbs elevated to maintenance; deficit temporarily paused
        Peak       = 4   // Pre-competition; highly individual
    }

    // ── DayProtocol ───────────────────────────────────────────────────────────

    /// <summary>
    /// Determines which macro template applies for the day.
    /// Training days carry higher carbohydrates than rest days.
    /// </summary>
    public enum DayProtocolType
    {
        TrainingDay = 0,
        RestDay     = 1,
        HighDay     = 2,  // Refeed / carb-up day — calories at maintenance
        DeloadDay   = 3   // Light-training day; reduced carbs vs TrainingDay
    }

    // ── Meal ──────────────────────────────────────────────────────────────────

    public enum MealTimingType
    {
        Breakfast  = 0,
        PreWorkout = 1,   // TimeFromTrainingMinutes will be negative
        PostWorkout = 2,  // TimeFromTrainingMinutes will be positive
        Lunch      = 3,
        Snack      = 4,
        Dinner     = 5,
        BeforeBed  = 6
    }

    // ── FoodItem ──────────────────────────────────────────────────────────────

    public enum FoodCategory
    {
        Protein      = 0,
        Carbohydrate = 1,
        Fat          = 2,
        Vegetable    = 3,
        Fruit        = 4,
        Dairy        = 5,
        Legume       = 6,
        Supplement   = 7,
        Other        = 8
    }

    // ── WeeklyCheckIn ─────────────────────────────────────────────────────────

    /// <summary>
    /// Category the client assigns to their optional note.
    /// Used to help the coach prioritise review and decide relevance.
    /// </summary>
    public enum NoteCategory
    {
        Health     = 0,  // Injury, illness, medical concern
        LifeEvent  = 1,  // Travel, Ramadan, major life disruption
        Feedback   = 2,  // Comment about the plan structure itself
        General    = 3   // Anything else the client considers worth mentioning
    }

    /// <summary>
    /// Records how the coach acted on the client's optional note.
    /// Logged permanently alongside the check-in.
    /// </summary>
    public enum CoachNoteAction
    {
        Acknowledged = 0,  // Coach read it; no plan change needed
        ActionTaken  = 1,  // Coach's decision was influenced by the note
        NotRelevant  = 2   // Coach determined note has no bearing on nutrition
    }

    /// <summary>
    /// Reflects how reliable the system considers the week's data for adjustment.
    ///   High   — adherence ≥ threshold, 2+ weeks of consistent history, clear trend.
    ///   Medium — borderline adherence, first 1–2 weeks, or one confounding factor.
    ///   Low    — adherence below threshold (data overridden by coach), or conflicting signals.
    /// </summary>
    public enum ProposalConfidence
    {
        High   = 0,
        Medium = 1,
        Low    = 2
    }

    /// <summary>Coach's decision on the system-generated weekly proposal.</summary>
    public enum CoachDecisionType
    {
        Approved  = 0,  // Accepted the system proposal exactly as generated
        Modified  = 1,  // Changed the kcal amount or macro vector
        Override  = 2,  // Ignored the proposal entirely; entered a custom adjustment
        Deferred  = 3   // No change this week — re-evaluate after next check-in
    }

    /// <summary>
    /// Determines which macro is adjusted first when calories change.
    /// Set per-client in ClientNutritionConstraints.
    /// </summary>
    public enum AdjustmentVector
    {
        RestDayCarbs     = 0,  // Preferred default: only rest-day carbohydrates move
        TrainingDayCarbs = 1,  // Only training-day carbohydrates move
        Fat              = 2,  // Fat is reduced/increased across all protocols
        Proportional     = 3   // Adjustment split between carbs and fat by macro ratio
    }

    // ── TraineeNutritionEnrollment ────────────────────────────────────────────

    public enum NutritionEnrollmentStatus
    {
        Active    = 0,
        Completed = 1,
        Cancelled = 2
    }
}
