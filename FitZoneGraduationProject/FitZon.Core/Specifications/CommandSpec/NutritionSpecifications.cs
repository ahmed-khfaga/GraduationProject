using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Specifications.Params;
//using FitZone.Service.DTOs.NutritionDTOs;
namespace FitZone.Core.Specifications.CommandSpec
{
    // ══════════════════════════════════════════════════════════════════════════
    // NUTRITION PLAN SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Published plans for the public catalogue with optional filtering.
    /// Includes Coach (for CoachName, CoachRating) so AutoMapper never lazy-loads.
    /// </summary>
    public class PublishedNutritionPlansSpec : BaseSpecatifications<NutritionPlan>
    {
        public PublishedNutritionPlansSpec(NutritionPlanFilterParams p) : base(n =>
            n.IsPublished &&
            (!p.Goal.HasValue         || n.TrainingGoal  == p.Goal)      &&
            (!p.Level.HasValue        || n.FitnessLevel  == p.Level)     &&
            (!p.Equipment.HasValue    || n.EquipmentType == p.Equipment) &&
            (!p.DurationWeeks.HasValue || n.DurationOnWeeks == p.DurationWeeks) &&
            (!p.LinkedToProgram.HasValue || (p.LinkedToProgram.Value
                ? n.LinkedWorkoutProgramID != null
                : n.LinkedWorkoutProgramID == null)))
        {
            Includes.Add(n => n.Coach);
            Includes.Add(n => n.Coach.ApplicationUser);

            if (p.Sort == "newest")
                OrderByDescending = n => n.PublishedAt!;
            else
                OrderBy = n => n.Name;

            ApplyPagination(p.PageIndex, p.PageSize);
        }

        // Count overload — no includes, no pagination, same predicate.
        public PublishedNutritionPlansSpec(NutritionPlanFilterParams p, bool countOnly) : base(n =>
            n.IsPublished &&
            (!p.Goal.HasValue         || n.TrainingGoal  == p.Goal)      &&
            (!p.Level.HasValue        || n.FitnessLevel  == p.Level)     &&
            (!p.Equipment.HasValue    || n.EquipmentType == p.Equipment) &&
            (!p.DurationWeeks.HasValue || n.DurationOnWeeks == p.DurationWeeks) &&
            (!p.LinkedToProgram.HasValue || (p.LinkedToProgram.Value
                ? n.LinkedWorkoutProgramID != null
                : n.LinkedWorkoutProgramID == null)))
        { }
    }

    /// <summary>All plans owned by a specific coach (published + drafts).</summary>
    public class CoachNutritionPlansSpec : BaseSpecatifications<NutritionPlan>
    {
        public CoachNutritionPlansSpec(int coachId) : base(n => n.CoachID == coachId)
        {
            Includes.Add(n => n.Coach);
            Includes.Add(n => n.Coach.ApplicationUser);
            OrderByDescending = n => n.Id;
        }
    }

    /// <summary>
    /// Full plan detail including all weeks.
    /// NutritionWeeks are included so AutoMapper can compute DayProtocolCount.
    /// Individual DayProtocols are NOT included here — load via NutritionWeekFullDetailSpec.
    /// </summary>
    public class NutritionPlanWithWeeksSpec : BaseSpecatifications<NutritionPlan>
    {
        public NutritionPlanWithWeeksSpec(int planId) : base(n => n.Id == planId)
        {
            Includes.Add(n => n.Coach);
            Includes.Add(n => n.Coach.ApplicationUser);
            Includes.Add(n => n.NutritionWeeks);
        }
    }

    /// <summary>
    /// Ownership check used before any coach mutation.
    /// Returns the plan only if it belongs to this coach — safe for all write operations.
    /// </summary>
    public class NutritionPlanByCoachSpec : BaseSpecatifications<NutritionPlan>
    {
        public NutritionPlanByCoachSpec(int planId, int coachId)
            : base(n => n.Id == planId && n.CoachID == coachId) { }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // NUTRITION WEEK SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Full week with all day protocols, meals, and food items.
    /// Used when serving week content to a trainee after the unlock gate passes.
    /// </summary>
    public class NutritionWeekFullDetailSpec : BaseSpecatifications<NutritionWeek>
    {
        public NutritionWeekFullDetailSpec(int planId, int weekNumber)
            : base(w => w.NutritionPlanID == planId && w.WeekNumber == weekNumber)
        {
            // NutritionPlan needed for ownership checks and DurationOnWeeks.
            Includes.Add(w => w.NutritionPlan);

            // Deep load the full week graph via string-based includes.
            // DayProtocols → Meals → MealFoodItems → FoodItem
            IncludeStrings.Add("DayProtocols");
            IncludeStrings.Add("DayProtocols.Meals");
            IncludeStrings.Add("DayProtocols.Meals.MealFoodItems");
            IncludeStrings.Add("DayProtocols.Meals.MealFoodItems.FoodItem");
        }
    }

    /// <summary>
    /// Loads a NutritionWeek with ownership verification.
    /// Used by coach mutation endpoints (update metadata, delete week).
    /// </summary>
    /// 
    public class NutritionWeekByCoachSpec : BaseSpecatifications<NutritionWeek>
    {
        public NutritionWeekByCoachSpec(int weekId, int coachId)
            : base(w => w.Id == weekId && w.NutritionPlan.CoachID == coachId)
        {
            Includes.Add(w => w.NutritionPlan);
        }
    }
    public class NutritionWeekByCoachWithDetailSpec : BaseSpecatifications<NutritionWeek>
    {
        public NutritionWeekByCoachWithDetailSpec(int weekId, int coachId)
            : base(w => w.Id == weekId && w.NutritionPlan.CoachID == coachId)
        {
            Includes.Add(w => w.NutritionPlan);
            IncludeStrings.Add("DayProtocols");
            IncludeStrings.Add("DayProtocols.Meals");
            IncludeStrings.Add("DayProtocols.Meals.MealFoodItems");
            IncludeStrings.Add("DayProtocols.Meals.MealFoodItems.FoodItem");
        }
    }

    /// <summary>
    /// Admin variant of NutritionWeekByCoachWithDetailSpec — no ownership filter,
    /// since an admin can inspect/manage any coach's plan content.
    /// </summary>
    public class NutritionWeekByIdWithDetailSpec : BaseSpecatifications<NutritionWeek>
    {
        public NutritionWeekByIdWithDetailSpec(int weekId)
            : base(w => w.Id == weekId)
        {
            Includes.Add(w => w.NutritionPlan);
            IncludeStrings.Add("DayProtocols");
            IncludeStrings.Add("DayProtocols.Meals");
            IncludeStrings.Add("DayProtocols.Meals.MealFoodItems");
            IncludeStrings.Add("DayProtocols.Meals.MealFoodItems.FoodItem");
        }
    }
    // ══════════════════════════════════════════════════════════════════════════
    // DAY PROTOCOL SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Full day protocol with meals and their food items.
    /// The deepest load in the system — used sparingly (only on demand per day).
    /// </summary>
    public class DayProtocolWithMealsSpec : BaseSpecatifications<DayProtocol>
    {
        public DayProtocolWithMealsSpec(int dayProtocolId)
            : base(d => d.Id == dayProtocolId)
        {
            Includes.Add(d => d.NutritionWeek);
            Includes.Add(d => d.NutritionWeek.NutritionPlan);
            // Deep load meals and food items via string includes.
            IncludeStrings.Add("Meals");
            IncludeStrings.Add("Meals.MealFoodItems");
            IncludeStrings.Add("Meals.MealFoodItems.FoodItem");
        }
    }

    /// <summary>
    /// Ownership verification for coach mutations on a DayProtocol.
    /// Traverses week → plan → coach to confirm ownership in one query.
    /// </summary>
    public class DayProtocolByCoachSpec : BaseSpecatifications<DayProtocol>
    {
        public DayProtocolByCoachSpec(int dayProtocolId, int coachId)
            : base(d => d.Id == dayProtocolId &&
                        d.NutritionWeek.NutritionPlan.CoachID == coachId)
        {
            Includes.Add(d => d.NutritionWeek);
            Includes.Add(d => d.NutritionWeek.NutritionPlan);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MEAL SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Meal with all food assignments.
    /// MealFoodItems include the FoodItem so macros can be computed.
    /// </summary>
    public class MealWithFoodItemsSpec : BaseSpecatifications<Meal>
    {
        public MealWithFoodItemsSpec(int mealId) : base(m => m.Id == mealId)
        {
            Includes.Add(m => m.DayProtocol);
            // Load MealFoodItems and their FoodItem for macro calculation.
            IncludeStrings.Add("MealFoodItems");
            IncludeStrings.Add("MealFoodItems.FoodItem");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // FOOD ITEM SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns all food items available to a coach:
    ///   — global items (CoachID == null, seeded by admin)
    ///   — items private to this coach (CoachID == coachId)
    /// Matches the exact predicate pattern of ExercisesForCoachSpec.
    /// </summary>
    public class FoodItemsForCoachSpec : BaseSpecatifications<FoodItem>
    {
        public FoodItemsForCoachSpec(int coachId, FoodItemFilterParams p) : base(f =>
            (f.CoachID == null || f.CoachID == coachId) &&
            (!p.Category.HasValue || f.Category == p.Category) &&
            (string.IsNullOrEmpty(p.Name)  || f.Name.Contains(p.Name)))
        {
            OrderBy = f => f.Name;
            ApplyPagination(p.PageIndex, p.PageSize);
        }

        // Count overload (no pagination)
        public FoodItemsForCoachSpec(int coachId, FoodItemFilterParams p, bool countOnly) : base(f =>
            (f.CoachID == null || f.CoachID == coachId) &&
            (!p.Category.HasValue || f.Category == p.Category) &&
            (string.IsNullOrEmpty(p.Name)  || f.Name.Contains(p.Name)))
        { }
    }

    /// <summary>
    /// Fetches a single food item — validates it is either global or owned by this coach.
    /// Used before edit/delete to prevent access to another coach's private items.
    /// </summary>
    public class FoodItemByIdForCoachSpec : BaseSpecatifications<FoodItem>
    {
        public FoodItemByIdForCoachSpec(int foodItemId, int coachId)
            : base(f => f.Id == foodItemId && (f.CoachID == null || f.CoachID == coachId)) { }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TRAINEE NUTRITION ENROLLMENT SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// All active nutrition enrollments for a trainee.
    /// Includes NutritionPlan so the service can compute CurrentAdjustedKcal display,
    /// and Coach.ApplicationUser for CoachName.
    /// </summary>
    public class TraineeActiveNutritionEnrollmentsSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public TraineeActiveNutritionEnrollmentsSpec(int traineeId)
            : base(e => e.TraineeID == traineeId && e.IsActive)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.NutritionPlan.Coach);
            Includes.Add(e => e.NutritionPlan.Coach.ApplicationUser);
            Includes.Add(e => e.Constraints);
        }
    }

    /// <summary>
    /// All enrollments (active + historical) for the enrollment history page.
    /// Ordered by StartDate descending (most recent first).
    /// </summary>
    public class TraineeAllNutritionEnrollmentsSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public TraineeAllNutritionEnrollmentsSpec(int traineeId)
            : base(e => e.TraineeID == traineeId)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.NutritionPlan.Coach);
            Includes.Add(e => e.NutritionPlan.Coach.ApplicationUser);
            OrderByDescending = e => e.StartDate;
        }
    }

    /// <summary>
    /// Fetches one enrollment by ID — validates it belongs to the given trainee.
    /// Used by week-access and session endpoints to prevent cross-trainee access.
    /// </summary>
    public class NutritionEnrollmentByIdSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public NutritionEnrollmentByIdSpec(int enrollmentId, int traineeId)
            : base(e => e.Id == enrollmentId && e.TraineeID == traineeId)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.Constraints);
        }
    }

    /// <summary>
    /// Fetches one nutrition enrollment by ID and verifies the plan belongs to coachId.
    /// Used by CheckInService and CoachReviewService to prevent cross-coach access.
    /// </summary>
    public class NutritionEnrollmentByIdForCoachSpec
        : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public NutritionEnrollmentByIdForCoachSpec(int enrollmentId, int coachId)
            : base(e => e.Id == enrollmentId &&
                        e.NutritionPlan.CoachID == coachId)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.Trainee);
            Includes.Add(e => e.Trainee.ApplicationUser);
            Includes.Add(e => e.Constraints);
        }
    }

    /// <summary>
    /// Checks whether the trainee already has an active enrollment in the specified plan.
    /// Used to enforce the one-active-per-plan unique constraint before inserting.
    /// (The partial unique DB index is the second enforcement layer.)
    /// </summary>
    public class ActiveNutritionEnrollmentByPlanSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public ActiveNutritionEnrollmentByPlanSpec(int traineeId, int planId)
            : base(e => e.TraineeID == traineeId && e.NutritionPlanID == planId && e.IsActive)
        {
            Includes.Add(e => e.NutritionPlan);
        }
    }

    /// <summary>
    /// Resume logic — finds the most recent inactive enrollment for this trainee in this plan.
    /// Ordered by MaxWeekUnlocked descending so FirstOrDefault returns the best saved progress.
    /// Mirrors PreviousEnrollmentInProgramSpec from the training system exactly.
    /// </summary>
    public class PreviousNutritionEnrollmentByPlanSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public PreviousNutritionEnrollmentByPlanSpec(int traineeId, int planId)
            : base(e => e.TraineeID == traineeId && e.NutritionPlanID == planId && !e.IsActive)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.Constraints);
            OrderByDescending = e => e.MaxWeekUnlocked;
        }
    }

    /// <summary>
    /// Fetches the enrollment that is linked to a specific WorkoutProgram enrollment.
    /// Used when checking bundle sync (nutrition week must not exceed training week).
    /// </summary>
    public class NutritionEnrollmentByWorkoutEnrollmentSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public NutritionEnrollmentByWorkoutEnrollmentSpec(int workoutEnrollmentId)
            : base(e => e.LinkedWorkoutEnrollmentID == workoutEnrollmentId && e.IsActive)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.Constraints);
        }
    }

    /// <summary>
    /// Coach review queue — all active enrollments across all trainees for plans
    /// owned by the specified coach, where a check-in has been submitted but
    /// the coach has not yet reviewed it this week.
    /// Used to build the weekly review inbox.
    /// </summary>
    public class PendingCoachReviewEnrollmentsSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public PendingCoachReviewEnrollmentsSpec(int coachId)
            : base(e => e.NutritionPlan.CoachID == coachId && e.IsActive)
        {
            Includes.Add(e => e.NutritionPlan);
            Includes.Add(e => e.Trainee);
            Includes.Add(e => e.Trainee.ApplicationUser);
            Includes.Add(e => e.Constraints);
            OrderByDescending = e => e.MaxWeekUnlocked;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // WEEKLY CHECK-IN SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fetches the check-in for a specific week in a specific enrollment.
    /// Used by the coach review panel and by the proposal engine to load the current record.
    /// </summary>
    public class CheckInByWeekSpec : BaseSpecatifications<WeeklyCheckIn>
    {
        public CheckInByWeekSpec(int enrollmentId, int weekNumber)
            : base(c => c.EnrollmentID == enrollmentId && c.WeekNumber == weekNumber)
        {
            Includes.Add(c => c.Enrollment);
            Includes.Add(c => c.Enrollment.NutritionPlan);
            Includes.Add(c => c.Enrollment.Constraints);
        }
    }

    /// <summary>
    /// All check-ins for a given enrollment, ordered by week number ascending.
    /// Used by the ProposalEngine to compute weight-delta history and identify trends.
    /// Includes no navigation properties — only the scalar fields are needed for calculation.
    /// </summary>
    public class AllCheckInsForEnrollmentSpec : BaseSpecatifications<WeeklyCheckIn>
    {
        public AllCheckInsForEnrollmentSpec(int enrollmentId)
            : base(c => c.EnrollmentID == enrollmentId)
        {
            OrderBy = c => c.WeekNumber;
        }
    }

    /// <summary>
    /// All check-ins awaiting coach review (submitted but CoachApprovedAt is null).
    /// Used to populate the coach's weekly review queue.
    /// </summary>
    public class PendingCheckInsForCoachSpec : BaseSpecatifications<WeeklyCheckIn>
    {
        public PendingCheckInsForCoachSpec(int coachId)
            : base(c => c.CoachApprovedAt == null &&
                        c.Enrollment.NutritionPlan.CoachID == coachId &&
                        c.Enrollment.IsActive)
        {
            Includes.Add(c => c.Enrollment);
            Includes.Add(c => c.Enrollment.NutritionPlan);
            Includes.Add(c => c.Enrollment.Trainee);
            Includes.Add(c => c.Enrollment.Trainee.ApplicationUser);
            Includes.Add(c => c.Enrollment.Constraints);
            OrderByDescending = c => c.SubmittedAt;
        }
    }

    /// <summary>
    /// The most recent check-in for a given enrollment.
    /// Used to compute PendingCheckIn and PendingCoachReview flags on the enrollment DTO.
    /// </summary>
    public class LatestCheckInForEnrollmentSpec : BaseSpecatifications<WeeklyCheckIn>
    {
        public LatestCheckInForEnrollmentSpec(int enrollmentId)
            : base(c => c.EnrollmentID == enrollmentId)
        {
            OrderByDescending = c => c.WeekNumber;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CLIENT NUTRITION CONSTRAINTS SPECIFICATIONS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fetches constraints for a specific enrollment.
    /// One-to-one: at most one result.
    /// </summary>
    public class ConstraintsByEnrollmentSpec : BaseSpecatifications<ClientNutritionConstraints>
    {
        public ConstraintsByEnrollmentSpec(int enrollmentId)
            : base(c => c.EnrollmentID == enrollmentId)
        {
            Includes.Add(c => c.Enrollment);
        }
    }
    /// <summary>
    /// All enrollments (active or historical) that reference a given plan.
    /// Used by AdminDeletePlanAsync to detect the FK block before attempting a
    /// delete that the database will reject anyway — surfaces a clear, actionable
    /// error message instead of a raw SqlException wrapped in a generic 500.
    /// </summary>
    public class EnrollmentsByPlanIdSpec : BaseSpecatifications<TraineeNutritionEnrollment>
    {
        public EnrollmentsByPlanIdSpec(int planId)
            : base(e => e.NutritionPlanID == planId)
        {
        }
    }
}
