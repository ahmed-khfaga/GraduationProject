using FitZone.Core.Entitys;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository.Data.Seed
{
    /// <summary>
    /// Seeds the complete nutrition system:
    ///   1. Global food item library (50+ whole foods across all categories)
    ///   2. Two nutrition plans authored by Coach Ahmed (Strength & Fat Loss)
    ///   3. Full week content — DayProtocols, Meals, MealFoodItems — for both plans
    ///   4. One active trainee enrollment (Mohamed Ali) with constraints set
    ///   5. Four weeks of weekly check-ins with coach decisions — simulates a
    ///      real 4-week coaching relationship from first check-in to week 4 unlock
    ///
    /// DEPENDENCIES (must run before this):
    ///   UserSeeder    → provides Coach Ahmed (ahmed.coach@fitzone.com) and
    ///                   Trainee Mohamed (mohamed@fitzone.com)
    ///   ProgramSeeder → provides "Push Pull Legs Foundation" to link workouts
    ///
    /// IDEMPOTENT: guarded by context.FoodItems.AnyAsync() — safe to re-run.
    /// </summary>
    public static class NutritionSeeder
    {
        public static async Task SeedAsync(FitContext context)
        {
            if (await context.FoodItems.AnyAsync()) return;

            // ── Resolve IDs from prior seeders ─────────────────────────────
            var coach = await context.Coachs
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUser.Email == "ahmed.coach@fitzone.com")
                ?? throw new InvalidOperationException("NutritionSeeder: Coach Ahmed not found. Run UserSeeder first.");

            var trainee = await context.Trainees
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(t => t.ApplicationUser.Email == "mohamed@fitzone.com")
                ?? throw new InvalidOperationException("NutritionSeeder: Trainee Mohamed not found. Run UserSeeder first.");

            // Optionally link to the workout program seeded by ProgramSeeder
            var workoutProgram = await context.WorkoutPrograms
                .FirstOrDefaultAsync(p => p.Name == "Push Pull Legs Foundation");

            // ── 1. GLOBAL FOOD LIBRARY ──────────────────────────────────────
            var foods = BuildFoodLibrary();
            context.FoodItems.AddRange(foods);
            await context.SaveChangesAsync();

            // Materialise a lookup by name for use below
            var foodMap = foods.ToDictionary(f => f.Name, f => f);

            // ── 2. NUTRITION PLANS ─────────────────────────────────────────
            await SeedMuscleBuildingPlan(context, coach.Id, workoutProgram?.Id, foodMap);
            await SeedFatLossPlan(context, coach.Id, null, foodMap);

            // ── 3. ENROLLMENT + CHECK-INS ──────────────────────────────────
            await SeedEnrollmentWithCheckIns(context, coach.Id, trainee.Id, foodMap);
        }

        // ══════════════════════════════════════════════════════════════════════
        // FOOD LIBRARY  (CoachID = null → global)
        // ══════════════════════════════════════════════════════════════════════

        private static List<FoodItem> BuildFoodLibrary() => new()
        {
            // ── PROTEIN SOURCES ────────────────────────────────────────────
            new FoodItem
            {
                CoachID = null, Name = "Chicken Breast (Skinless)", Category = FoodCategory.Protein,
                CaloriesPer100g = 165m, ProteinPer100g = 31m, CarbPer100g = 0m, FatPer100g = 3.6m, FiberPer100g = 0m,
                ServingSizeG = 150, ServingSizeName = "1 medium breast", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Lean Ground Beef (90/10)", Category = FoodCategory.Protein,
                CaloriesPer100g = 176m, ProteinPer100g = 26m, CarbPer100g = 0m, FatPer100g = 8m, FiberPer100g = 0m,
                ServingSizeG = 100, ServingSizeName = "100g raw", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Salmon Fillet", Category = FoodCategory.Protein,
                CaloriesPer100g = 208m, ProteinPer100g = 20m, CarbPer100g = 0m, FatPer100g = 13m, FiberPer100g = 0m,
                ServingSizeG = 150, ServingSizeName = "1 medium fillet", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Tuna (Canned in Water)", Category = FoodCategory.Protein,
                CaloriesPer100g = 116m, ProteinPer100g = 26m, CarbPer100g = 0m, FatPer100g = 1m, FiberPer100g = 0m,
                ServingSizeG = 120, ServingSizeName = "1 can drained", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Whole Eggs", Category = FoodCategory.Protein,
                CaloriesPer100g = 143m, ProteinPer100g = 13m, CarbPer100g = 1m, FatPer100g = 10m, FiberPer100g = 0m,
                ServingSizeG = 50, ServingSizeName = "1 large egg", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Egg Whites", Category = FoodCategory.Protein,
                CaloriesPer100g = 52m, ProteinPer100g = 11m, CarbPer100g = 0.7m, FatPer100g = 0.2m, FiberPer100g = 0m,
                ServingSizeG = 100, ServingSizeName = "3 egg whites (~100ml)", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Turkey Breast (Sliced)", Category = FoodCategory.Protein,
                CaloriesPer100g = 135m, ProteinPer100g = 29m, CarbPer100g = 0m, FatPer100g = 1.5m, FiberPer100g = 0m,
                ServingSizeG = 100, ServingSizeName = "100g", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Shrimp", Category = FoodCategory.Protein,
                CaloriesPer100g = 99m, ProteinPer100g = 24m, CarbPer100g = 0.2m, FatPer100g = 0.3m, FiberPer100g = 0m,
                ServingSizeG = 100, ServingSizeName = "100g cooked", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Whey Protein Isolate", Category = FoodCategory.Supplement,
                CaloriesPer100g = 370m, ProteinPer100g = 90m, CarbPer100g = 3m, FatPer100g = 1m, FiberPer100g = 0m,
                ServingSizeG = 30, ServingSizeName = "1 scoop (30g)", IsWhole = false
            },

            // ── CARBOHYDRATE SOURCES ───────────────────────────────────────
            new FoodItem
            {
                CoachID = null, Name = "White Rice (Cooked)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 130m, ProteinPer100g = 2.7m, CarbPer100g = 28m, FatPer100g = 0.3m, FiberPer100g = 0.4m,
                ServingSizeG = 200, ServingSizeName = "1 cup cooked", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Brown Rice (Cooked)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 112m, ProteinPer100g = 2.6m, CarbPer100g = 23m, FatPer100g = 0.9m, FiberPer100g = 1.8m,
                ServingSizeG = 200, ServingSizeName = "1 cup cooked", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Rolled Oats (Dry)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 389m, ProteinPer100g = 17m, CarbPer100g = 66m, FatPer100g = 7m, FiberPer100g = 10m,
                ServingSizeG = 80, ServingSizeName = "1 cup dry oats", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Sweet Potato (Baked)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 90m, ProteinPer100g = 2m, CarbPer100g = 21m, FatPer100g = 0.1m, FiberPer100g = 3.3m,
                ServingSizeG = 150, ServingSizeName = "1 medium baked", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Whole Wheat Bread", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 247m, ProteinPer100g = 13m, CarbPer100g = 41m, FatPer100g = 3.4m, FiberPer100g = 7m,
                ServingSizeG = 40, ServingSizeName = "2 slices", IsWhole = false
            },
            new FoodItem
            {
                CoachID = null, Name = "Quinoa (Cooked)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 120m, ProteinPer100g = 4.4m, CarbPer100g = 22m, FatPer100g = 1.9m, FiberPer100g = 2.8m,
                ServingSizeG = 185, ServingSizeName = "1 cup cooked", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Pasta (Whole Wheat, Cooked)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 124m, ProteinPer100g = 5.3m, CarbPer100g = 27m, FatPer100g = 0.5m, FiberPer100g = 3.9m,
                ServingSizeG = 200, ServingSizeName = "1 cup cooked", IsWhole = false
            },
            new FoodItem
            {
                CoachID = null, Name = "White Potato (Boiled)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 78m, ProteinPer100g = 2m, CarbPer100g = 17m, FatPer100g = 0.1m, FiberPer100g = 1.8m,
                ServingSizeG = 200, ServingSizeName = "1 medium potato", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Rice Cakes (Plain)", Category = FoodCategory.Carbohydrate,
                CaloriesPer100g = 387m, ProteinPer100g = 8m, CarbPer100g = 81m, FatPer100g = 3m, FiberPer100g = 0.5m,
                ServingSizeG = 30, ServingSizeName = "3 cakes", IsWhole = false
            },
            new FoodItem
            {
                CoachID = null, Name = "Banana", Category = FoodCategory.Fruit,
                CaloriesPer100g = 89m, ProteinPer100g = 1.1m, CarbPer100g = 23m, FatPer100g = 0.3m, FiberPer100g = 2.6m,
                ServingSizeG = 120, ServingSizeName = "1 medium banana", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Apple", Category = FoodCategory.Fruit,
                CaloriesPer100g = 52m, ProteinPer100g = 0.3m, CarbPer100g = 14m, FatPer100g = 0.2m, FiberPer100g = 2.4m,
                ServingSizeG = 180, ServingSizeName = "1 medium apple", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Berries (Mixed, Frozen)", Category = FoodCategory.Fruit,
                CaloriesPer100g = 52m, ProteinPer100g = 0.7m, CarbPer100g = 12m, FatPer100g = 0.3m, FiberPer100g = 2.5m,
                ServingSizeG = 100, ServingSizeName = "100g", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Dates (Medjool)", Category = FoodCategory.Fruit,
                CaloriesPer100g = 277m, ProteinPer100g = 1.8m, CarbPer100g = 75m, FatPer100g = 0.2m, FiberPer100g = 6.7m,
                ServingSizeG = 24, ServingSizeName = "1 large date", IsWhole = true
            },

            // ── FAT SOURCES ────────────────────────────────────────────────
            new FoodItem
            {
                CoachID = null, Name = "Olive Oil (Extra Virgin)", Category = FoodCategory.Fat,
                CaloriesPer100g = 884m, ProteinPer100g = 0m, CarbPer100g = 0m, FatPer100g = 100m, FiberPer100g = 0m,
                ServingSizeG = 14, ServingSizeName = "1 tablespoon", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Almonds (Raw)", Category = FoodCategory.Fat,
                CaloriesPer100g = 579m, ProteinPer100g = 21m, CarbPer100g = 22m, FatPer100g = 50m, FiberPer100g = 12.5m,
                ServingSizeG = 28, ServingSizeName = "1 handful (28g)", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Avocado", Category = FoodCategory.Fat,
                CaloriesPer100g = 160m, ProteinPer100g = 2m, CarbPer100g = 9m, FatPer100g = 15m, FiberPer100g = 7m,
                ServingSizeG = 150, ServingSizeName = "1/2 large avocado", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Natural Peanut Butter", Category = FoodCategory.Fat,
                CaloriesPer100g = 588m, ProteinPer100g = 25m, CarbPer100g = 20m, FatPer100g = 50m, FiberPer100g = 6m,
                ServingSizeG = 32, ServingSizeName = "2 tablespoons", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Walnuts", Category = FoodCategory.Fat,
                CaloriesPer100g = 654m, ProteinPer100g = 15m, CarbPer100g = 14m, FatPer100g = 65m, FiberPer100g = 6.7m,
                ServingSizeG = 28, ServingSizeName = "1 handful (28g)", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Flaxseed (Ground)", Category = FoodCategory.Fat,
                CaloriesPer100g = 534m, ProteinPer100g = 18m, CarbPer100g = 29m, FatPer100g = 42m, FiberPer100g = 27m,
                ServingSizeG = 14, ServingSizeName = "1 tablespoon", IsWhole = true
            },

            // ── DAIRY & DAIRY-ADJACENT ─────────────────────────────────────
            new FoodItem
            {
                CoachID = null, Name = "Greek Yogurt (0% Fat, Plain)", Category = FoodCategory.Dairy,
                CaloriesPer100g = 59m, ProteinPer100g = 10m, CarbPer100g = 3.6m, FatPer100g = 0.4m, FiberPer100g = 0m,
                ServingSizeG = 200, ServingSizeName = "1 cup", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Cottage Cheese (Low Fat)", Category = FoodCategory.Dairy,
                CaloriesPer100g = 72m, ProteinPer100g = 12m, CarbPer100g = 3m, FatPer100g = 1m, FiberPer100g = 0m,
                ServingSizeG = 200, ServingSizeName = "1 cup", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Low-Fat Milk", Category = FoodCategory.Dairy,
                CaloriesPer100g = 42m, ProteinPer100g = 3.4m, CarbPer100g = 5m, FatPer100g = 1m, FiberPer100g = 0m,
                ServingSizeG = 240, ServingSizeName = "1 glass (240ml)", IsWhole = true
            },

            // ── VEGETABLES ─────────────────────────────────────────────────
            new FoodItem
            {
                CoachID = null, Name = "Broccoli (Steamed)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 35m, ProteinPer100g = 2.4m, CarbPer100g = 7m, FatPer100g = 0.4m, FiberPer100g = 2.6m,
                ServingSizeG = 150, ServingSizeName = "1 cup florets", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Spinach (Raw)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 23m, ProteinPer100g = 2.9m, CarbPer100g = 3.6m, FatPer100g = 0.4m, FiberPer100g = 2.2m,
                ServingSizeG = 100, ServingSizeName = "2 large handfuls", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Cucumber", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 16m, ProteinPer100g = 0.7m, CarbPer100g = 4m, FatPer100g = 0.1m, FiberPer100g = 0.5m,
                ServingSizeG = 150, ServingSizeName = "1/2 medium cucumber", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Tomatoes (Cherry)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 18m, ProteinPer100g = 0.9m, CarbPer100g = 3.9m, FatPer100g = 0.2m, FiberPer100g = 1.2m,
                ServingSizeG = 100, ServingSizeName = "6–8 cherry tomatoes", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Bell Pepper (Red)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 31m, ProteinPer100g = 1m, CarbPer100g = 6m, FatPer100g = 0.3m, FiberPer100g = 2.1m,
                ServingSizeG = 120, ServingSizeName = "1 medium pepper", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Asparagus (Steamed)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 20m, ProteinPer100g = 2.2m, CarbPer100g = 3.9m, FatPer100g = 0.1m, FiberPer100g = 2.1m,
                ServingSizeG = 100, ServingSizeName = "5 spears", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Green Beans (Steamed)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 31m, ProteinPer100g = 1.8m, CarbPer100g = 7m, FatPer100g = 0.2m, FiberPer100g = 3.4m,
                ServingSizeG = 150, ServingSizeName = "1 cup", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Zucchini (Grilled)", Category = FoodCategory.Vegetable,
                CaloriesPer100g = 17m, ProteinPer100g = 1.2m, CarbPer100g = 3.1m, FatPer100g = 0.3m, FiberPer100g = 1m,
                ServingSizeG = 150, ServingSizeName = "1 medium zucchini", IsWhole = true
            },

            // ── LEGUMES ────────────────────────────────────────────────────
            new FoodItem
            {
                CoachID = null, Name = "Lentils (Boiled)", Category = FoodCategory.Legume,
                CaloriesPer100g = 116m, ProteinPer100g = 9m, CarbPer100g = 20m, FatPer100g = 0.4m, FiberPer100g = 7.9m,
                ServingSizeG = 200, ServingSizeName = "1 cup cooked", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Chickpeas (Canned, Drained)", Category = FoodCategory.Legume,
                CaloriesPer100g = 164m, ProteinPer100g = 8.9m, CarbPer100g = 27m, FatPer100g = 2.6m, FiberPer100g = 7.6m,
                ServingSizeG = 165, ServingSizeName = "1 cup drained", IsWhole = true
            },
            new FoodItem
            {
                CoachID = null, Name = "Black Beans (Canned, Drained)", Category = FoodCategory.Legume,
                CaloriesPer100g = 132m, ProteinPer100g = 8.9m, CarbPer100g = 24m, FatPer100g = 0.5m, FiberPer100g = 8.7m,
                ServingSizeG = 165, ServingSizeName = "1 cup drained", IsWhole = true
            },
        };

        // ══════════════════════════════════════════════════════════════════════
        // PLAN 1 — MUSCLE BUILDING (Linked to PPL workout)
        // 4 weeks, TDEE+300, 2.0g protein/kg
        // ══════════════════════════════════════════════════════════════════════

        private static async Task SeedMuscleBuildingPlan(
            FitContext context, int coachId, int? workoutProgramId,
            Dictionary<string, FoodItem> foods)
        {
            var plan = new NutritionPlan
            {
                CoachID = coachId,
                LinkedWorkoutProgramID = workoutProgramId,
                Name = "Hypertrophy Nutrition Foundation",
                Description = "A structured 4-week muscle-building nutrition plan designed to pair with a Push/Pull/Legs split. " +
                                         "Uses a moderate calorie surplus calibrated to your individual TDEE so you gain lean mass without excessive fat accumulation.",
                ExpectedOutcome = "After 4 weeks your training performance will be visibly supported by your diet, " +
                                         "you will have established a consistent meal structure, and you should see 0.5–1 kg of body-weight gain — primarily lean mass.",
                NextSteps = "Transition to a longer hypertrophy block (8–12 weeks) with progressive overload in both training and calories, " +
                                         "or enter a short 4-week maintenance phase before your next muscle-building cycle.",
                TrainingGoal = TrainingGoal.BuildMuscle,
                FitnessLevel = FitnessLevel.Intermediate,
                EquipmentType = EquipmentType.FullGym,
                DurationOnWeeks = 4,
                CalorieStrategyType = CalorieStrategyType.TDEERelative,
                TDEEAdjustmentKcal = +300,
                ProteinTargetPerKg = 2.0m,
                IsPublished = true,
                PublishedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            };

            context.NutritionPlans.Add(plan);
            await context.SaveChangesAsync();

            // ── 4 weeks ────────────────────────────────────────────────────
            var weeks = new[]
            {
                new NutritionWeek
                {
                    NutritionPlanID  = plan.Id,
                    WeekNumber       = 1,
                    WeekProtocolType = WeekProtocolType.Standard,
                    CalorieModifier  = 0m,
                    WeekDescription  = "Baseline week — establish your eating rhythm and identify any practical challenges (timing, food prep, appetite).",
                    FocusNote        = "Nail breakfast and post-workout meals first. Those two windows drive the most muscle-protein synthesis. Everything else can be flexible this week.",
                    ProgressionNote  = "Week 1 is diagnostic. If you cannot hit protein by Friday, the problem is almost always meal spacing — eating too few large meals instead of 4–5 moderate ones.",
                    NextWeekPreview  = "Week 2 holds the same calorie target but I adjust your carbohydrate distribution — rest days drop slightly to redirect fuel to training days."
                },
                new NutritionWeek
                {
                    NutritionPlanID  = plan.Id,
                    WeekNumber       = 2,
                    WeekProtocolType = WeekProtocolType.Standard,
                    CalorieModifier  = 0m,
                    WeekDescription  = "Carbohydrate cycling begins — training-day carbs increase 15%, rest-day carbs decrease 15%. Total weekly calories stay identical.",
                    FocusNote        = "Your pre-workout meal is your most important carbohydrate meal. Eat it 90–120 minutes before you train. This is not negotiable.",
                    ProgressionNote  = "Carb cycling improves insulin sensitivity and makes surplus calories more anabolic. Your scale weight may fluctuate ±1.5 kg — this is glycogen and water, not fat.",
                    NextWeekPreview  = "Week 3 is a high-volume simulation week: training intensity spikes and so does your calorie target (+8%). Prepare to eat more."
                },
                new NutritionWeek
                {
                    NutritionPlanID  = plan.Id,
                    WeekNumber       = 3,
                    WeekProtocolType = WeekProtocolType.HighVolume,
                    CalorieModifier  = 0.08m,
                    WeekDescription  = "High-volume week — calorie target increases by 8%. This matches the elevated training demand and maximises anabolic signalling.",
                    FocusNote        = "Add an extra carbohydrate serving to both the pre- and post-workout windows. The additional calories go here first, not into fat.",
                    ProgressionNote  = "Muscle-protein synthesis peaks in the 24–48 hours after a hard session. If you train Monday, Wednesday, and Friday — Tuesday and Thursday lunches are critical for recovery.",
                    NextWeekPreview  = "Week 4 is a planned deload: training volume drops and so do calories (-10%). This is not a punishment — it is scheduled recovery that makes you stronger."
                },
                new NutritionWeek
                {
                    NutritionPlanID  = plan.Id,
                    WeekNumber       = 4,
                    WeekProtocolType = WeekProtocolType.Deload,
                    CalorieModifier  = -0.10m,
                    WeekDescription  = "Deload week — reduce training volume and calories by 10%. Let inflammation resolve, refill glycogen, and consolidate the adaptations from weeks 1–3.",
                    FocusNote        = "Keep protein identical to week 3. Only carbohydrates and fats drop. Protein is the last macro you ever cut.",
                    ProgressionNote  = "You will likely feel stronger and better-rested by Thursday of deload week. That subjective feeling correlates strongly with real performance improvements. Trust the process.",
                    NextWeekPreview  = "Program complete. Your check-in this week determines whether you enter a new 8-week muscle block or a 2-week maintenance transition before the next phase."
                }
            };

            context.NutritionWeeks.AddRange(weeks);
            await context.SaveChangesAsync();

            // ── Build day protocols for EACH week ─────────────────────────

            // Training days: Mon, Wed, Fri → 2850 kcal / P:180 C:340 F:70
            // Rest    days: Tue, Thu, Sat   → 2550 kcal / P:180 C:270 F:75
            // Note: actual enrolled values will be adjusted by TDEE; these are the
            //       plan-template targets a 90kg trainee at TDEE ~2550 would see.

            foreach (var week in weeks)
            {
                decimal calMod = week.CalorieModifier;
                int trainCal = (int)(2850m * (1 + calMod));
                int restCal = (int)(2550m * (1 + calMod));
                int trainCarb = (int)(340m * (1 + calMod));
                int restCarb = (int)(270m * (1 + calMod));

                // Monday — Push Training Day
                var mon = new DayProtocol
                {
                    NutritionWeekID = week.Id,
                    DayProtocolType = DayProtocolType.TrainingDay,
                    WeekDay = WeekDay.Monday,
                    TotalCaloriesTarget = trainCal,
                    ProteinTargetG = 180,
                    CarbTargetG = trainCarb,
                    FatTargetG = 70,
                    ProtocolNotes = "Push day. Pre-workout meal 90 min before. Post-workout shake immediately after, rice + chicken within 60 min."
                };

                // Tuesday — Rest Day
                var tue = new DayProtocol
                {
                    NutritionWeekID = week.Id,
                    DayProtocolType = DayProtocolType.RestDay,
                    WeekDay = WeekDay.Tuesday,
                    TotalCaloriesTarget = restCal,
                    ProteinTargetG = 180,
                    CarbTargetG = restCarb,
                    FatTargetG = 75,
                    ProtocolNotes = "Rest day. Lower carbs, slightly higher fat. Focus on recovery foods: salmon, avocado, berries."
                };

                // Wednesday — Pull Training Day
                var wed = new DayProtocol
                {
                    NutritionWeekID = week.Id,
                    DayProtocolType = DayProtocolType.TrainingDay,
                    WeekDay = WeekDay.Wednesday,
                    TotalCaloriesTarget = trainCal,
                    ProteinTargetG = 180,
                    CarbTargetG = trainCarb,
                    FatTargetG = 70,
                    ProtocolNotes = "Pull day. Same structure as Monday. Deadlifts are in this session — eat your pre-workout meal on time."
                };

                // Thursday — Rest Day
                var thu = new DayProtocol
                {
                    NutritionWeekID = week.Id,
                    DayProtocolType = DayProtocolType.RestDay,
                    WeekDay = WeekDay.Thursday,
                    TotalCaloriesTarget = restCal,
                    ProteinTargetG = 180,
                    CarbTargetG = restCarb,
                    FatTargetG = 75,
                    ProtocolNotes = "Rest day. Prioritise sleep and hydration. This is the recovery day between pull and legs."
                };

                // Friday — Legs Training Day
                var fri = new DayProtocol
                {
                    NutritionWeekID = week.Id,
                    DayProtocolType = DayProtocolType.TrainingDay,
                    WeekDay = WeekDay.Friday,
                    TotalCaloriesTarget = trainCal,
                    ProteinTargetG = 180,
                    CarbTargetG = trainCarb,
                    FatTargetG = 70,
                    ProtocolNotes = "Legs day — heaviest lower-body session. Add an extra 50g of sweet potato to your pre-workout meal to fuel squats and RDLs."
                };

                // Saturday — Rest Day
                var sat = new DayProtocol
                {
                    NutritionWeekID = week.Id,
                    DayProtocolType = DayProtocolType.RestDay,
                    WeekDay = WeekDay.Saturday,
                    TotalCaloriesTarget = restCal,
                    ProteinTargetG = 180,
                    CarbTargetG = restCarb,
                    FatTargetG = 75,
                    ProtocolNotes = "Weekend rest day. Meal timing is relaxed — you do not have to eat pre/post workout windows. Keep protein consistent."
                };

                context.DayProtocols.AddRange(mon, tue, wed, thu, fri, sat);
                await context.SaveChangesAsync();

                // Seed meals for each protocol (Mon only fully detailed here;
                // other days use the same pattern but are also fully seeded below)
                await SeedTrainingDayMeals(context, mon, foods, isLegDay: false);
                await SeedRestDayMeals(context, tue, foods);
                await SeedTrainingDayMeals(context, wed, foods, isLegDay: false);
                await SeedRestDayMeals(context, thu, foods);
                await SeedTrainingDayMeals(context, fri, foods, isLegDay: true);
                await SeedRestDayMeals(context, sat, foods);
            }
        }

        // ── Meals: Training Day ────────────────────────────────────────────

        private static async Task SeedTrainingDayMeals(
            FitContext context, DayProtocol protocol,
            Dictionary<string, FoodItem> foods, bool isLegDay)
        {
            // MEAL 1 — Breakfast (730 kcal / P45 C90 F18)
            var breakfast = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Breakfast",
                TimingType = MealTimingType.Breakfast,
                MealOrder = 1,
                TargetCalories = 730,
                TargetProteinG = 45,
                TargetCarbG = 90,
                TargetFatG = 18,
                Notes = "Eat within 45 minutes of waking. Oats expand stomach volume, suppressing hunger until pre-workout. Do NOT skip this meal."
            };

            // MEAL 2 — Pre-Workout (600 kcal / P35 C80 F12)
            var preWorkout = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Pre-Workout",
                TimingType = MealTimingType.PreWorkout,
                MealOrder = 2,
                TimeFromTrainingMinutes = -90,
                TargetCalories = 600,
                TargetProteinG = 35,
                TargetCarbG = isLegDay ? 90 : 80,
                TargetFatG = 12,
                Notes = isLegDay
                    ? "Leg day — add 50g extra sweet potato (about half a small one). Legs sessions deplete glycogen faster than upper body."
                    : "90 minutes before training. Chicken and rice digest fast enough to fuel the session without bloating."
            };

            // MEAL 3 — Post-Workout (450 kcal / P45 C55 F5)
            var postWorkout = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Post-Workout",
                TimingType = MealTimingType.PostWorkout,
                MealOrder = 3,
                TimeFromTrainingMinutes = 30,
                TargetCalories = 450,
                TargetProteinG = 45,
                TargetCarbG = 55,
                TargetFatG = 5,
                Notes = "Within 30 minutes post-session. Whey absorbs in 20–30 min. Banana provides fast glucose to restore glycogen. Keep fat low here — fat slows protein absorption."
            };

            // MEAL 4 — Lunch (600 kcal / P40 C65 F18)
            var lunch = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Lunch",
                TimingType = MealTimingType.Lunch,
                MealOrder = 4,
                TargetCalories = 600,
                TargetProteinG = 40,
                TargetCarbG = 65,
                TargetFatG = 18,
                Notes = "Midday recovery meal. Brown rice digests slower than white — ideal now that the acute post-workout window has closed."
            };

            // MEAL 5 — Dinner (470 kcal / P35 C50 F17)
            var dinner = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Dinner",
                TimingType = MealTimingType.Dinner,
                MealOrder = 5,
                TargetCalories = 470,
                TargetProteinG = 35,
                TargetCarbG = 50,
                TargetFatG = 17,
                Notes = "Keep dinner balanced. Salmon provides omega-3s that reduce training-induced inflammation overnight."
            };

            context.Meals.AddRange(breakfast, preWorkout, postWorkout, lunch, dinner);
            await context.SaveChangesAsync();

            // ── Food assignments ────────────────────────────────────────────

            // Breakfast items
            var breakfastItems = new List<MealFoodItem>
            {
                new() { MealID = breakfast.Id, FoodItemID = foods["Rolled Oats (Dry)"].Id,           AmountGrams = 80,  IsOptional = false, SwapGroupID = null }, // main carb
                new() { MealID = breakfast.Id, FoodItemID = foods["Egg Whites"].Id,                  AmountGrams = 200, IsOptional = false, SwapGroupID = null }, // protein #1
                new() { MealID = breakfast.Id, FoodItemID = foods["Whole Eggs"].Id,                  AmountGrams = 50,  IsOptional = false, SwapGroupID = null }, // protein #2 + fat
                new() { MealID = breakfast.Id, FoodItemID = foods["Berries (Mixed, Frozen)"].Id,     AmountGrams = 100, IsOptional = false, SwapGroupID = null }, // micronutrients
                new() { MealID = breakfast.Id, FoodItemID = foods["Greek Yogurt (0% Fat, Plain)"].Id,AmountGrams = 100, IsOptional = true,  SwapGroupID = 1    }, // optional swap A
                new() { MealID = breakfast.Id, FoodItemID = foods["Cottage Cheese (Low Fat)"].Id,    AmountGrams = 100, IsOptional = true,  SwapGroupID = 1    }, // optional swap A alt
            };

            // Pre-workout items
            var preworkoutItems = new List<MealFoodItem>
            {
                new() { MealID = preWorkout.Id, FoodItemID = foods["Chicken Breast (Skinless)"].Id, AmountGrams = 150, IsOptional = false, SwapGroupID = null },
                new() { MealID = preWorkout.Id, FoodItemID = foods["White Rice (Cooked)"].Id,       AmountGrams = 200, IsOptional = false, SwapGroupID = 2    }, // carb swap group
                new() { MealID = preWorkout.Id, FoodItemID = foods["Sweet Potato (Baked)"].Id,      AmountGrams = 150, IsOptional = true,  SwapGroupID = 2    }, // carb swap alt
                new() { MealID = preWorkout.Id, FoodItemID = foods["Broccoli (Steamed)"].Id,        AmountGrams = 100, IsOptional = false, SwapGroupID = null },
            };

            // Post-workout items
            var postworkoutItems = new List<MealFoodItem>
            {
                new() { MealID = postWorkout.Id, FoodItemID = foods["Whey Protein Isolate"].Id, AmountGrams = 30,  IsOptional = false, SwapGroupID = null },
                new() { MealID = postWorkout.Id, FoodItemID = foods["Banana"].Id,               AmountGrams = 120, IsOptional = false, SwapGroupID = null },
                new() { MealID = postWorkout.Id, FoodItemID = foods["Dates (Medjool)"].Id,      AmountGrams = 48,  IsOptional = true,  SwapGroupID = null }, // extra fast carbs optional
            };

            // Lunch items
            var lunchItems = new List<MealFoodItem>
            {
                new() { MealID = lunch.Id, FoodItemID = foods["Lean Ground Beef (90/10)"].Id, AmountGrams = 150, IsOptional = false, SwapGroupID = 3    }, // protein swap
                new() { MealID = lunch.Id, FoodItemID = foods["Turkey Breast (Sliced)"].Id,   AmountGrams = 150, IsOptional = true,  SwapGroupID = 3    }, // protein swap alt
                new() { MealID = lunch.Id, FoodItemID = foods["Brown Rice (Cooked)"].Id,      AmountGrams = 200, IsOptional = false, SwapGroupID = null },
                new() { MealID = lunch.Id, FoodItemID = foods["Spinach (Raw)"].Id,            AmountGrams = 100, IsOptional = false, SwapGroupID = null },
                new() { MealID = lunch.Id, FoodItemID = foods["Olive Oil (Extra Virgin)"].Id, AmountGrams = 10,  IsOptional = false, SwapGroupID = null },
                new() { MealID = lunch.Id, FoodItemID = foods["Tomatoes (Cherry)"].Id,        AmountGrams = 100, IsOptional = false, SwapGroupID = null },
            };

            // Dinner items
            var dinnerItems = new List<MealFoodItem>
            {
                new() { MealID = dinner.Id, FoodItemID = foods["Salmon Fillet"].Id,           AmountGrams = 150, IsOptional = false, SwapGroupID = null },
                new() { MealID = dinner.Id, FoodItemID = foods["Sweet Potato (Baked)"].Id,    AmountGrams = 180, IsOptional = false, SwapGroupID = 4    }, // carb swap
                new() { MealID = dinner.Id, FoodItemID = foods["Quinoa (Cooked)"].Id,         AmountGrams = 185, IsOptional = true,  SwapGroupID = 4    }, // carb swap alt
                new() { MealID = dinner.Id, FoodItemID = foods["Asparagus (Steamed)"].Id,     AmountGrams = 100, IsOptional = false, SwapGroupID = null },
                new() { MealID = dinner.Id, FoodItemID = foods["Bell Pepper (Red)"].Id,       AmountGrams = 60,  IsOptional = true,  SwapGroupID = null },
            };

            context.MealFoodItems.AddRange(breakfastItems);
            context.MealFoodItems.AddRange(preworkoutItems);
            context.MealFoodItems.AddRange(postworkoutItems);
            context.MealFoodItems.AddRange(lunchItems);
            context.MealFoodItems.AddRange(dinnerItems);
            await context.SaveChangesAsync();
        }

        // ── Meals: Rest Day ────────────────────────────────────────────────

        private static async Task SeedRestDayMeals(
            FitContext context, DayProtocol protocol,
            Dictionary<string, FoodItem> foods)
        {
            var breakfast = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Breakfast",
                TimingType = MealTimingType.Breakfast,
                MealOrder = 1,
                TargetCalories = 600,
                TargetProteinG = 45,
                TargetCarbG = 60,
                TargetFatG = 20,
                Notes = "Rest day breakfast. Eggs + oats. No rush. Eat within 60 min of waking."
            };
            var lunch = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Lunch",
                TimingType = MealTimingType.Lunch,
                MealOrder = 2,
                TargetCalories = 650,
                TargetProteinG = 45,
                TargetCarbG = 65,
                TargetFatG = 22,
                Notes = "Main meal of the rest day. Salmon + sweet potato. Omega-3 from salmon accelerates inter-session recovery."
            };
            var snack = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Afternoon Snack",
                TimingType = MealTimingType.Snack,
                MealOrder = 3,
                TargetCalories = 350,
                TargetProteinG = 25,
                TargetCarbG = 30,
                TargetFatG = 12,
                Notes = "Bridge meal to keep protein synthesis going. Greek yogurt + almonds is fast to prepare and hits both macros."
            };
            var dinner = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Dinner",
                TimingType = MealTimingType.Dinner,
                MealOrder = 4,
                TargetCalories = 600,
                TargetProteinG = 45,
                TargetCarbG = 55,
                TargetFatG = 18,
                Notes = "Keep it simple. Chicken or ground beef with rice and greens. Consistent structure reduces decision fatigue."
            };
            var beforeBed = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Before Bed",
                TimingType = MealTimingType.BeforeBed,
                MealOrder = 5,
                TargetCalories = 250,
                TargetProteinG = 25,
                TargetCarbG = 20,
                TargetFatG = 5,
                Notes = "Casein-rich before bed. Cottage cheese digests slowly, sustaining amino acid release through the 7–8 hour overnight fast. Critical for muscle repair."
            };

            context.Meals.AddRange(breakfast, lunch, snack, dinner, beforeBed);
            await context.SaveChangesAsync();

            var items = new List<MealFoodItem>
            {
                // Breakfast
                new() { MealID = breakfast.Id, FoodItemID = foods["Whole Eggs"].Id,                   AmountGrams = 150, IsOptional = false },
                new() { MealID = breakfast.Id, FoodItemID = foods["Rolled Oats (Dry)"].Id,            AmountGrams = 70,  IsOptional = false },
                new() { MealID = breakfast.Id, FoodItemID = foods["Berries (Mixed, Frozen)"].Id,      AmountGrams = 100, IsOptional = false },
                new() { MealID = breakfast.Id, FoodItemID = foods["Natural Peanut Butter"].Id,        AmountGrams = 15,  IsOptional = true  },
                // Lunch
                new() { MealID = lunch.Id, FoodItemID = foods["Salmon Fillet"].Id,                    AmountGrams = 150, IsOptional = false },
                new() { MealID = lunch.Id, FoodItemID = foods["Sweet Potato (Baked)"].Id,             AmountGrams = 200, IsOptional = false },
                new() { MealID = lunch.Id, FoodItemID = foods["Broccoli (Steamed)"].Id,               AmountGrams = 150, IsOptional = false },
                new() { MealID = lunch.Id, FoodItemID = foods["Olive Oil (Extra Virgin)"].Id,         AmountGrams = 10,  IsOptional = false },
                new() { MealID = lunch.Id, FoodItemID = foods["Avocado"].Id,                          AmountGrams = 75,  IsOptional = true  },
                // Snack
                new() { MealID = snack.Id, FoodItemID = foods["Greek Yogurt (0% Fat, Plain)"].Id,     AmountGrams = 200, IsOptional = false },
                new() { MealID = snack.Id, FoodItemID = foods["Almonds (Raw)"].Id,                    AmountGrams = 28,  IsOptional = false },
                new() { MealID = snack.Id, FoodItemID = foods["Apple"].Id,                            AmountGrams = 180, IsOptional = true  },
                // Dinner
                new() { MealID = dinner.Id, FoodItemID = foods["Chicken Breast (Skinless)"].Id,       AmountGrams = 150, IsOptional = false },
                new() { MealID = dinner.Id, FoodItemID = foods["White Rice (Cooked)"].Id,             AmountGrams = 200, IsOptional = false },
                new() { MealID = dinner.Id, FoodItemID = foods["Spinach (Raw)"].Id,                   AmountGrams = 100, IsOptional = false },
                new() { MealID = dinner.Id, FoodItemID = foods["Olive Oil (Extra Virgin)"].Id,        AmountGrams = 10,  IsOptional = false },
                new() { MealID = dinner.Id, FoodItemID = foods["Tomatoes (Cherry)"].Id,               AmountGrams = 100, IsOptional = false },
                // Before Bed
                new() { MealID = beforeBed.Id, FoodItemID = foods["Cottage Cheese (Low Fat)"].Id,     AmountGrams = 200, IsOptional = false },
                new() { MealID = beforeBed.Id, FoodItemID = foods["Flaxseed (Ground)"].Id,            AmountGrams = 14,  IsOptional = true  },
            };

            context.MealFoodItems.AddRange(items);
            await context.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════════
        // PLAN 2 — FAT LOSS
        // 8 weeks, TDEE-400, 2.2g protein/kg
        // ══════════════════════════════════════════════════════════════════════

        private static async Task SeedFatLossPlan(
            FitContext context, int coachId, int? workoutProgramId,
            Dictionary<string, FoodItem> foods)
        {
            var plan = new NutritionPlan
            {
                CoachID = coachId,
                LinkedWorkoutProgramID = workoutProgramId,
                Name = "Sustainable Fat Loss Protocol",
                Description = "An 8-week fat-loss plan built around a moderate calorie deficit calibrated to your TDEE. " +
                                         "High protein prevents lean-mass loss while the deficit drives fat oxidation at a sustainable rate.",
                ExpectedOutcome = "Expect 2.5–4 kg of fat loss over 8 weeks while maintaining or slightly increasing strength. " +
                                         "Body composition improves measurably. Rate of loss averages 0.4–0.6 kg per week.",
                NextSteps = "Transition to a 4–6 week maintenance phase to recalibrate your TDEE before entering the next fat-loss block. " +
                                         "Attempting a second consecutive deficit without a maintenance break stalls fat loss.",
                TrainingGoal = TrainingGoal.LoseFat,
                FitnessLevel = FitnessLevel.Beginner,
                EquipmentType = EquipmentType.FullGym,
                DurationOnWeeks = 8,
                CalorieStrategyType = CalorieStrategyType.TDEERelative,
                TDEEAdjustmentKcal = -400,
                ProteinTargetPerKg = 2.2m,
                IsPublished = true,
                PublishedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            context.NutritionPlans.Add(plan);
            await context.SaveChangesAsync();

            // Seed 8 nutrition weeks — standard protocol with progressive adjustments
            var weekConfigs = new[]
            {
                (1, WeekProtocolType.Standard,   0.00m, "Establish baseline. Measure everything. Do not guess at portion sizes in week 1."),
                (2, WeekProtocolType.Standard,   0.00m, "Same targets. Review your food log for common compliance failures and fix them."),
                (3, WeekProtocolType.Standard,   0.00m, "If weight loss is on track (0.4–0.6 kg), hold calories. If stalled, check adherence first."),
                (4, WeekProtocolType.Refeed,     0.10m, "Refeed week — calories rise to near maintenance. This is planned, not a break. Leptin restores and metabolism resets."),
                (5, WeekProtocolType.Standard,   0.00m, "Return to deficit. Post-refeed week often shows accelerated fat loss. Stay disciplined."),
                (6, WeekProtocolType.Standard,  -0.05m, "Mild additional reduction (-5%). Applied only if progress has been slower than expected in weeks 3–5."),
                (7, WeekProtocolType.HighVolume,  0.05m, "Training intensity peaks. Minor calorie increase (+5%) to support performance while maintaining deficit."),
                (8, WeekProtocolType.Deload,    -0.08m, "Final deload. Body composition assessment week. Reduce training and calories. Prepare for maintenance transition.")
            };

            foreach (var (wkNum, protocol, calMod, desc) in weekConfigs)
            {
                var wk = new NutritionWeek
                {
                    NutritionPlanID = plan.Id,
                    WeekNumber = wkNum,
                    WeekProtocolType = protocol,
                    CalorieModifier = calMod,
                    WeekDescription = desc,
                    FocusNote = wkNum == 4
                        ? "Refeed is not a cheat day. Hit your carbohydrate target precisely — overeat and you eliminate the benefit."
                        : wkNum <= 3
                            ? "Protein and vegetable adherence are the two non-negotiables. Everything else can flex."
                            : "Consistency beats perfection. A 90% adherent week every week outperforms a perfect week followed by a poor week.",
                    ProgressionNote = $"Week {wkNum} of 8. You are {(int)((wkNum / 8.0) * 100)}% through the program.",
                    NextWeekPreview = wkNum < 8
                        ? $"Week {wkNum + 1}: {weekConfigs[wkNum].Item3}"
                        : "Program complete. Enter a maintenance phase immediately."
                };

                context.NutritionWeeks.Add(wk);
                await context.SaveChangesAsync();

                // Fat loss day protocols — 2 training days, 2 rest days per week sample
                // Training: 2000 kcal (pre-TDEE adjustment), P:180 C:180 F:60
                // Rest:     1750 kcal                        P:180 C:130 F:60

                int trainCal = (int)(2000m * (1 + calMod));
                int restCal = (int)(1750m * (1 + calMod));
                int trainCarb = (int)(180m * (1 + calMod));
                int restCarb = (int)(130m * (1 + calMod));

                var fatLossTrainingDay = new DayProtocol
                {
                    NutritionWeekID = wk.Id,
                    DayProtocolType = DayProtocolType.TrainingDay,
                    WeekDay = WeekDay.Monday,
                    TotalCaloriesTarget = trainCal,
                    ProteinTargetG = 180,
                    CarbTargetG = trainCarb,
                    FatTargetG = 60,
                    ProtocolNotes = "Training day. Pre-workout is mandatory. Post-workout shake is mandatory. No exceptions."
                };

                var fatLossRestDay = new DayProtocol
                {
                    NutritionWeekID = wk.Id,
                    DayProtocolType = DayProtocolType.RestDay,
                    WeekDay = WeekDay.Tuesday,
                    TotalCaloriesTarget = restCal,
                    ProteinTargetG = 180,
                    CarbTargetG = restCarb,
                    FatTargetG = 60,
                    ProtocolNotes = "Rest day. Vegetables fill volume without calories. Eat broccoli, spinach, and cucumber freely — they are your satiety tools."
                };

                context.DayProtocols.AddRange(fatLossTrainingDay, fatLossRestDay);
                await context.SaveChangesAsync();

                await SeedFatLossTrainingDayMeals(context, fatLossTrainingDay, foods);
                await SeedFatLossRestDayMeals(context, fatLossRestDay, foods);
            }
        }

        private static async Task SeedFatLossTrainingDayMeals(
            FitContext context, DayProtocol protocol,
            Dictionary<string, FoodItem> foods)
        {
            var breakfast = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Breakfast",
                TimingType = MealTimingType.Breakfast,
                MealOrder = 1,
                TargetCalories = 500,
                TargetProteinG = 50,
                TargetCarbG = 45,
                TargetFatG = 10,
                Notes = "High protein, moderate carb breakfast. Egg whites are your best friend on a fat-loss plan — maximum protein per calorie."
            };
            var preWorkout = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Pre-Workout",
                TimingType = MealTimingType.PreWorkout,
                MealOrder = 2,
                TimeFromTrainingMinutes = -75,
                TargetCalories = 350,
                TargetProteinG = 30,
                TargetCarbG = 45,
                TargetFatG = 5,
                Notes = "75 min before training. Small and digestible. Banana + protein source. No fat — slows gastric emptying."
            };
            var postWorkout = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Post-Workout",
                TimingType = MealTimingType.PostWorkout,
                MealOrder = 3,
                TimeFromTrainingMinutes = 30,
                TargetCalories = 350,
                TargetProteinG = 40,
                TargetCarbG = 35,
                TargetFatG = 5,
                Notes = "Fast protein + moderate carb. On a deficit, this window is critical — skip it and you risk muscle catabolism."
            };
            var dinner = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Dinner",
                TimingType = MealTimingType.Dinner,
                MealOrder = 4,
                TargetCalories = 500,
                TargetProteinG = 40,
                TargetCarbG = 45,
                TargetFatG = 15,
                Notes = "Largest meal of the day volume-wise. Fill half your plate with vegetables first."
            };
            var beforeBed = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Before Bed",
                TimingType = MealTimingType.BeforeBed,
                MealOrder = 5,
                TargetCalories = 200,
                TargetProteinG = 20,
                TargetCarbG = 10,
                TargetFatG = 5,
                Notes = "Cottage cheese before bed. Slow-digesting casein protects muscle overnight while you are in a deficit."
            };

            context.Meals.AddRange(breakfast, preWorkout, postWorkout, dinner, beforeBed);
            await context.SaveChangesAsync();

            var items = new List<MealFoodItem>
            {
                new() { MealID = breakfast.Id,   FoodItemID = foods["Egg Whites"].Id,                  AmountGrams = 250, IsOptional = false },
                new() { MealID = breakfast.Id,   FoodItemID = foods["Whole Eggs"].Id,                  AmountGrams = 50,  IsOptional = false },
                new() { MealID = breakfast.Id,   FoodItemID = foods["Rolled Oats (Dry)"].Id,           AmountGrams = 50,  IsOptional = false },
                new() { MealID = breakfast.Id,   FoodItemID = foods["Spinach (Raw)"].Id,               AmountGrams = 100, IsOptional = false },
                new() { MealID = preWorkout.Id,  FoodItemID = foods["Chicken Breast (Skinless)"].Id,   AmountGrams = 120, IsOptional = false },
                new() { MealID = preWorkout.Id,  FoodItemID = foods["Banana"].Id,                      AmountGrams = 120, IsOptional = false },
                new() { MealID = preWorkout.Id,  FoodItemID = foods["Rice Cakes (Plain)"].Id,          AmountGrams = 30,  IsOptional = true  },
                new() { MealID = postWorkout.Id, FoodItemID = foods["Whey Protein Isolate"].Id,        AmountGrams = 30,  IsOptional = false },
                new() { MealID = postWorkout.Id, FoodItemID = foods["White Rice (Cooked)"].Id,         AmountGrams = 150, IsOptional = false },
                new() { MealID = dinner.Id,      FoodItemID = foods["Turkey Breast (Sliced)"].Id,      AmountGrams = 180, IsOptional = false, SwapGroupID = 5 },
                new() { MealID = dinner.Id,      FoodItemID = foods["Tuna (Canned in Water)"].Id,      AmountGrams = 150, IsOptional = true,  SwapGroupID = 5 },
                new() { MealID = dinner.Id,      FoodItemID = foods["Sweet Potato (Baked)"].Id,        AmountGrams = 180, IsOptional = false },
                new() { MealID = dinner.Id,      FoodItemID = foods["Broccoli (Steamed)"].Id,          AmountGrams = 200, IsOptional = false },
                new() { MealID = dinner.Id,      FoodItemID = foods["Green Beans (Steamed)"].Id,       AmountGrams = 100, IsOptional = false },
                new() { MealID = dinner.Id,      FoodItemID = foods["Olive Oil (Extra Virgin)"].Id,    AmountGrams = 7,   IsOptional = false },
                new() { MealID = beforeBed.Id,   FoodItemID = foods["Cottage Cheese (Low Fat)"].Id,    AmountGrams = 200, IsOptional = false },
                new() { MealID = beforeBed.Id,   FoodItemID = foods["Flaxseed (Ground)"].Id,           AmountGrams = 7,   IsOptional = true  },
            };

            context.MealFoodItems.AddRange(items);
            await context.SaveChangesAsync();
        }

        private static async Task SeedFatLossRestDayMeals(
            FitContext context, DayProtocol protocol,
            Dictionary<string, FoodItem> foods)
        {
            var breakfast = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Breakfast",
                TimingType = MealTimingType.Breakfast,
                MealOrder = 1,
                TargetCalories = 450,
                TargetProteinG = 45,
                TargetCarbG = 35,
                TargetFatG = 12,
                Notes = "Lighter carbs on rest day. Protein stays the same. Fat is slightly higher to maintain satiety without training-day fuel requirements."
            };
            var lunch = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Lunch",
                TimingType = MealTimingType.Lunch,
                MealOrder = 2,
                TargetCalories = 450,
                TargetProteinG = 45,
                TargetCarbG = 35,
                TargetFatG = 14,
                Notes = "Tuna + legumes on rest days. High satiety, high fiber, slower digestion keeps hunger suppressed longer."
            };
            var snack = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Snack",
                TimingType = MealTimingType.Snack,
                MealOrder = 3,
                TargetCalories = 200,
                TargetProteinG = 20,
                TargetCarbG = 15,
                TargetFatG = 5,
                Notes = "Small bridge snack. If you are not hungry, skip it — your body is using stored fat for fuel, which is the goal."
            };
            var dinner = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Dinner",
                TimingType = MealTimingType.Dinner,
                MealOrder = 4,
                TargetCalories = 450,
                TargetProteinG = 45,
                TargetCarbG = 30,
                TargetFatG = 14,
                Notes = "Rest day dinner. Salmon omega-3s support fat metabolism and reduce cortisol-related water retention."
            };
            var beforeBed = new Meal
            {
                DayProtocolID = protocol.Id,
                Name = "Before Bed",
                TimingType = MealTimingType.BeforeBed,
                MealOrder = 5,
                TargetCalories = 200,
                TargetProteinG = 25,
                TargetCarbG = 15,
                TargetFatG = 5,
                Notes = "Greek yogurt before bed on rest days. Lower calorie than cottage cheese, still provides casein."
            };

            context.Meals.AddRange(breakfast, lunch, snack, dinner, beforeBed);
            await context.SaveChangesAsync();

            var items = new List<MealFoodItem>
            {
                new() { MealID = breakfast.Id, FoodItemID = foods["Egg Whites"].Id,                   AmountGrams = 250, IsOptional = false },
                new() { MealID = breakfast.Id, FoodItemID = foods["Whole Eggs"].Id,                   AmountGrams = 50,  IsOptional = false },
                new() { MealID = breakfast.Id, FoodItemID = foods["Rolled Oats (Dry)"].Id,            AmountGrams = 40,  IsOptional = false },
                new() { MealID = breakfast.Id, FoodItemID = foods["Berries (Mixed, Frozen)"].Id,      AmountGrams = 100, IsOptional = false },
                new() { MealID = lunch.Id,     FoodItemID = foods["Tuna (Canned in Water)"].Id,       AmountGrams = 150, IsOptional = false },
                new() { MealID = lunch.Id,     FoodItemID = foods["Chickpeas (Canned, Drained)"].Id,  AmountGrams = 100, IsOptional = false },
                new() { MealID = lunch.Id,     FoodItemID = foods["Cucumber"].Id,                     AmountGrams = 150, IsOptional = false },
                new() { MealID = lunch.Id,     FoodItemID = foods["Tomatoes (Cherry)"].Id,            AmountGrams = 100, IsOptional = false },
                new() { MealID = lunch.Id,     FoodItemID = foods["Olive Oil (Extra Virgin)"].Id,     AmountGrams = 10,  IsOptional = false },
                new() { MealID = snack.Id,     FoodItemID = foods["Greek Yogurt (0% Fat, Plain)"].Id, AmountGrams = 150, IsOptional = false },
                new() { MealID = snack.Id,     FoodItemID = foods["Apple"].Id,                        AmountGrams = 120, IsOptional = true  },
                new() { MealID = dinner.Id,    FoodItemID = foods["Salmon Fillet"].Id,                AmountGrams = 150, IsOptional = false },
                new() { MealID = dinner.Id,    FoodItemID = foods["Lentils (Boiled)"].Id,             AmountGrams = 150, IsOptional = false },
                new() { MealID = dinner.Id,    FoodItemID = foods["Zucchini (Grilled)"].Id,           AmountGrams = 150, IsOptional = false },
                new() { MealID = dinner.Id,    FoodItemID = foods["Spinach (Raw)"].Id,                AmountGrams = 80,  IsOptional = false },
                new() { MealID = beforeBed.Id, FoodItemID = foods["Greek Yogurt (0% Fat, Plain)"].Id, AmountGrams = 200, IsOptional = false },
            };

            context.MealFoodItems.AddRange(items);
            await context.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ENROLLMENT + 4 WEEKS OF CHECK-INS
        //
        // Scenario: Mohamed Ali (82kg Male, 178cm, 28yo)
        //   TDEE ≈ 2550 kcal (Harris-Benedict + moderate activity)
        //   Plan: Hypertrophy Nutrition Foundation (TDEE+300 = 2850 kcal target)
        //   Baseline protein: 82 × 2.0 = 164g
        //   Enrolled: 6 weeks ago (StartDate back-dated so Week 4 is naturally unlocked)
        //
        // Week-by-week narrative (coach + trainee exchange):
        //   Wk 1: Compliant. +0.7 kg scale. Coach approves — on track.
        //   Wk 2: Slightly under-eating. +0.3 kg only. Coach adds 100 kcal.
        //   Wk 3: High-volume week. Gained 1.1 kg (glycogen + food). Coach defers — expected noise.
        //   Wk 4: Deload. Weight stabilises at 83.9 kg. Coach approves final week unlock.
        // ══════════════════════════════════════════════════════════════════════

        private static async Task SeedEnrollmentWithCheckIns(
            FitContext context, int coachId, int traineeId,
            Dictionary<string, FoodItem> foods)
        {
            // Resolve plan
            var plan = await context.NutritionPlans
                .FirstOrDefaultAsync(p => p.Name == "Hypertrophy Nutrition Foundation")
                ?? throw new InvalidOperationException("Hypertrophy plan not found.");

            // Back-date StartDate so the natural time gate already unlocks Week 4
            var startDate = DateTime.UtcNow.AddDays(-28);

            var enrollment = new TraineeNutritionEnrollment
            {
                TraineeID = traineeId,
                NutritionPlanID = plan.Id,
                StartDate = startDate,
                Status = NutritionEnrollmentStatus.Active,
                IsActive = true,
                MaxWeekUnlocked = 4,       // weeks 1–3 check-ins approved, week 4 unlocked
                BaselineCalories = 2850,    // TDEE(2550) + 300
                CurrentAdjustedKcal = 2950,    // +100 added after Week 2 check-in
                EmpiricalTDEEKcal = 2560     // computed after Week 4 data (close to Harris-Benedict)
            };

            context.TraineeNutritionEnrollments.Add(enrollment);
            await context.SaveChangesAsync();

            // ── Constraints (set by coach at enrollment) ───────────────────
            var constraints = new ClientNutritionConstraints
            {
                EnrollmentID = enrollment.Id,
                WeightAveragingDays = 3,
                ExpectedWeeklyChangeMin = 0.20m,   // Gaining: 200–500g/wk target
                ExpectedWeeklyChangeMax = 0.50m,
                DeviationTriggerKg = 0.15m,
                ProteinFloorG = 160,
                FatFloorG = 55,
                CalorieFloor = 2400,
                CalorieCeiling = 3500,
                MaxSingleAdjustmentKcal = 150,
                MaxCumulativeDriftKcal = 400,
                PreferredAdjustmentVector = AdjustmentVector.RestDayCarbs,
                AdherenceThresholdPercent = 75,
                RequireConsecutiveWeeksDeviation = false,
                ApplyTrainingWeekNoiseCorrection = true,
                EnergyLevelEscalationRule = true,
                PreserveLeanMassOverRate = false,
                EnableBaselineRecalibrationReview = true
            };

            context.ClientNutritionConstraints.Add(constraints);
            await context.SaveChangesAsync();

            // ── WEEK 1 CHECK-IN ────────────────────────────────────────────
            // Submitted end of Week 1. Weight: 82.1 / 82.3 / 82.4 → Avg 82.27
            // Delta vs baseline (82.0): +0.27 kg — within range. Clean data.
            // Coach decision: Approved (system proposal: hold calories).
            var checkIn1 = new WeeklyCheckIn
            {
                EnrollmentID = enrollment.Id,
                WeekNumber = 1,
                SubmittedAt = startDate.AddDays(7),
                MorningWeight1 = 82.1m,
                MorningWeight2 = 82.3m,
                MorningWeight3 = 82.4m,
                AverageWeight = 82.27m,
                EnergyLevel = 4,
                HungerLevel = 3,
                SleepQuality = 4,
                AdherencePercent = 88,
                ClientNote = "First week felt manageable. Pre-workout meal timing was tricky on Wednesday because I had a late meeting.",
                NoteCategory = NoteCategory.Feedback,
                // System proposal
                SystemProposalKcal = 2850,
                SystemProposalReasoning = "Weight gain of +0.27 kg is within the target range of +0.20–0.50 kg/wk. " +
                                              "Adherence at 88% is above the 75% threshold. Energy and sleep are strong. " +
                                              "No adjustment warranted. Hold current calorie target.",
                ProjectedOutcomeIfNoAction = "Continued at current rate, client is on track to gain 1.1–1.6 kg over the 4-week program.",
                SystemConfidence = ProposalConfidence.High,
                // Coach decision
                CoachDecision = CoachDecisionType.Approved,
                FinalAdjustmentKcal = 0,
                AppliedAdjustmentVector = AdjustmentVector.RestDayCarbs,
                CoachNote = "Solid first week, Mohamed. Pre-workout timing: set a calendar reminder 90 min before your session. That solves the Wednesday problem.",
                CoachNoteAction = CoachNoteAction.Acknowledged,
                CoachReviewedAt = startDate.AddDays(8),
                CoachApprovedAt = startDate.AddDays(8)   // Week 2 gate OPEN
            };

            // ── WEEK 2 CHECK-IN ────────────────────────────────────────────
            // Weight: 82.4 / 82.5 / 82.6 → Avg 82.5. Delta: +0.23 kg vs wk1 avg.
            // Scale looks fine but adherence dropped to 78%. Low-end of acceptable.
            // System flags: borderline adherence, weight technically in range but lower end.
            // Coach decision: Modified — adds 100 kcal to rest-day carbs.
            var checkIn2 = new WeeklyCheckIn
            {
                EnrollmentID = enrollment.Id,
                WeekNumber = 2,
                SubmittedAt = startDate.AddDays(14),
                MorningWeight1 = 82.4m,
                MorningWeight2 = 82.5m,
                MorningWeight3 = 82.6m,
                AverageWeight = 82.5m,
                EnergyLevel = 3,
                HungerLevel = 4,    // higher hunger = may be under-eating
                SleepQuality = 3,
                AdherencePercent = 78,
                ClientNote = "Hunger was noticeably higher this week, especially on rest days in the evening. Ate out on Saturday and went over.",
                NoteCategory = NoteCategory.Feedback,
                SystemProposalKcal = 0,    // algorithm holds: weight in range, adherence borderline
                SystemProposalReasoning = "Weight delta of +0.23 kg is at the lower end of the target range (+0.20–0.50). " +
                                         "Adherence at 78% is above threshold but hunger score of 4/5 and energy score of 3/5 suggest possible under-fuelling. " +
                                         "Confidence: Medium. Algorithm holds calories but flags for coach review.",
                ProjectedOutcomeIfNoAction = "If adherence drops below 75% next week, algorithm will return Low confidence and defer. " +
                                             "Client's hunger score trending upward — risk of dietary compliance breakdown in week 3.",
                SystemConfidence = ProposalConfidence.Medium,
                CoachDecision = CoachDecisionType.Modified,
                FinalAdjustmentKcal = 100,   // +100 kcal above system proposal
                AppliedAdjustmentVector = AdjustmentVector.RestDayCarbs,
                CoachNote = "Added 100 kcal to rest-day carbs — add one extra rice cake and a piece of fruit in the evening. " ,
                CoachNoteAction = CoachNoteAction.ActionTaken,
                CoachReviewedAt = startDate.AddDays(15),
                CoachApprovedAt = startDate.AddDays(15)   // Week 3 gate OPEN; CurrentAdjustedKcal → 2950
            };

            // ── WEEK 3 CHECK-IN ────────────────────────────────────────────
            // High-volume week. Weight spikes: 83.1 / 83.5 / 84.0 → Avg 83.53
            // Delta vs wk2: +1.03 kg — well above upper bound.
            // BUT: this is a planned high-volume week with +8% calories and heavy training.
            // System flags noise-correction override: water + glycogen masking.
            // Coach: Deferred — correct call. Do not cut calories during high-volume week.
            var checkIn3 = new WeeklyCheckIn
            {
                EnrollmentID = enrollment.Id,
                WeekNumber = 3,
                SubmittedAt = startDate.AddDays(21),
                MorningWeight1 = 83.1m,
                MorningWeight2 = 83.5m,
                MorningWeight3 = 84.0m,
                AverageWeight = 83.53m,
                EnergyLevel = 5,    // energy excellent — high-volume fuelled correctly
                HungerLevel = 3,
                SleepQuality = 4,
                AdherencePercent = 91,
                ClientNote = "Best training week I have had. Hit all PRs. Weight jumped which is worrying me a bit.",
                NoteCategory = NoteCategory.Feedback,
                SystemProposalKcal = -150,
                SystemProposalReasoning = "Weight delta of +1.03 kg significantly exceeds the upper bound of +0.50 kg/wk. " +
                                         "HOWEVER: ApplyTrainingWeekNoiseCorrection = true. " +
                                         "This is a designated HighVolume week with CalorieModifier +8%. " +
                                         "High training volume causes muscle micro-damage → acute inflammation → water retention of 0.5–1.5 kg. " +
                                         "This weight is not adipose tissue. Adherence at 91% is the highest of the program. " +
                                         "Algorithm confidence is Low — do not act on this reading. Defer.",
                ProjectedOutcomeIfNoAction = "If training noise is masking true weight, the scale will correct -0.5 to -1.0 kg during deload week 4 without any calorie change.",
                SystemConfidence = ProposalConfidence.Low,
                CoachDecision = CoachDecisionType.Deferred,
                FinalAdjustmentKcal = 0,
                AppliedAdjustmentVector = AdjustmentVector.RestDayCarbs,
                CoachNote = "Mohamed — the weight spike is exactly what I expected. You trained harder than any previous week, you ate more carbs (glycogen), " +
                                             "and your muscles are inflamed from the volume. That is 0.8–1.2 kg of water and glycogen — not fat. " +
                                             "Calories unchanged going into deload. Trust the process.",
                CoachNoteAction = CoachNoteAction.Acknowledged,
                CoachReviewedAt = startDate.AddDays(22),
                CoachApprovedAt = startDate.AddDays(22)   // Week 4 gate OPEN
            };

            // ── WEEK 4 CHECK-IN ────────────────────────────────────────────
            // Deload week. Weight drops as predicted: 83.9 / 83.7 / 83.8 → Avg 83.8
            // 4-week net gain: 83.8 - 82.0 = +1.8 kg. Expected for a lean bulk.
            // EmpiricalTDEE calibrated from 4 weeks of data: 2560 kcal (very close to Harris-Benedict).
            // Coach: Approved. Program complete.
            var checkIn4 = new WeeklyCheckIn
            {
                EnrollmentID = enrollment.Id,
                WeekNumber = 4,
                SubmittedAt = startDate.AddDays(28),
                MorningWeight1 = 83.9m,
                MorningWeight2 = 83.7m,
                MorningWeight3 = 83.8m,
                AverageWeight = 83.8m,
                EnergyLevel = 4,
                HungerLevel = 2,
                SleepQuality = 5,
                AdherencePercent = 90,
                ClientNote = "Weight came back down like you said it would. Feel lighter and stronger. Clothes fit differently.",
                NoteCategory = NoteCategory.Feedback,
                SystemProposalKcal = 0,
                SystemProposalReasoning = "Final program week. 4-week net gain: +1.8 kg from 82.0 to 83.8. " +
                                         "Target was +0.8–2.0 kg lean mass over 4 weeks at +300 kcal/day. Outcome is within target range. " +
                                         "Empirical TDEE from 4-week data: 2560 kcal (vs Harris-Benedict estimate of 2550 — extremely close). " +
                                         "No adjustment needed. Program completes this week.",
                ProjectedOutcomeIfNoAction = "Program is complete. Recommend maintenance transition at 2560–2600 kcal for 3–4 weeks before entering the next block.",
                SystemConfidence = ProposalConfidence.High,
                CoachDecision = CoachDecisionType.Approved,
                FinalAdjustmentKcal = 0,
                AppliedAdjustmentVector = AdjustmentVector.RestDayCarbs,
                CoachNote = "Excellent execution, Mohamed. 4-week total: +1.8 kg — right in the lean-gain window. " +
                                             "Your TDEE is 2560 — remember this number. Maintenance: eat 2560. " +
                                             "Next block starts in 3 weeks. Enjoy a maintenance break. You earned it.",
                CoachNoteAction = CoachNoteAction.Acknowledged,
                CoachReviewedAt = startDate.AddDays(29),
                CoachApprovedAt = startDate.AddDays(29)   // Program complete
            };

            context.WeeklyCheckIns.AddRange(checkIn1, checkIn2, checkIn3, checkIn4);
            await context.SaveChangesAsync();
        }
    }
}