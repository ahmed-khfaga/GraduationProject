using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    /// <summary>
    /// Pure calculation service — zero database access.
    /// Every method is synchronous arithmetic. Registered as Scoped to match the
    /// other services even though it holds no state itself.
    ///
    /// Equation: Mifflin-St Jeor (1990) — current clinical standard.
    ///   Male   : (10 × kg) + (6.25 × cm) − (5 × age) + 5
    ///   Female : (10 × kg) + (6.25 × cm) − (5 × age) − 161
    /// </summary>
    public class TDEEService : ITDEEService
    {
        // ── Multiplicative activity levels ─────────────────────────────────
        private static readonly Dictionary<int, decimal> _activityMultipliers = new()
        {
            { 0, 1.20m },  // sedentary / rest-only
            { 1, 1.20m },  // 1 session / week
            { 2, 1.375m }, // 2 sessions / week
            { 3, 1.55m },  // 3–4 sessions / week (threshold = 3)
            { 4, 1.55m },
            { 5, 1.725m }, // 5–6 sessions / week
            { 6, 1.725m },
        };

        // ── Goal-based calorie adjustments (kcal/day) ──────────────────────
        private static readonly Dictionary<TrainingGoal, int> _goalAdjustments = new()
        {
            { TrainingGoal.LoseFat,          -400 },
            { TrainingGoal.BuildMuscle,       +250 },
            { TrainingGoal.GetStronger,       +50  },
            { TrainingGoal.ImproveEndurance,  +100 },
            { TrainingGoal.MoveBetter,          0  },
            { TrainingGoal.GeneralFitness,   -200 },
            { TrainingGoal.MaintainWeight,     0   },
        };

        // ── ITDEEService ────────────────────────────────────────────────────

        public int CalculateBMR(string gender, decimal weightKg, decimal heightCm, int ageYears)
        {
            decimal bmr = (10m * weightKg) + (6.25m * heightCm) - (5m * ageYears);
            bmr += gender.Trim().ToLowerInvariant() == "male" ? 5m : -161m;
            return (int)Math.Round(bmr, MidpointRounding.AwayFromZero);
        }

        public int ApplyActivityMultiplier(int bmr, int sessionsPerWeek)
        {
            var key    = Math.Min(sessionsPerWeek, 6);
            var factor = _activityMultipliers.GetValueOrDefault(key, 1.9m);
            if (sessionsPerWeek >= 7) factor = 1.9m;
            return (int)Math.Round(bmr * factor, MidpointRounding.AwayFromZero);
        }

        public int ApplyGoalAdjustment(int tdee, TrainingGoal goal)
        {
            var delta = _goalAdjustments.GetValueOrDefault(goal, 0);
            return tdee + delta;
        }

        public (int proteinG, int carbG, int fatG) CalculateMacros(
            int     adjustedCalories,
            decimal weightKg,
            decimal proteinTargetPerKg,
            int     fatFloorG)
        {
            // Protein — primary lever, always calculated first.
            int proteinG    = (int)Math.Round(weightKg * proteinTargetPerKg, MidpointRounding.AwayFromZero);
            int proteinKcal = proteinG * 4;

            // Fat — minimum 25 % of calories or the hard floor, whichever is greater.
            int fatFromRatio = (int)Math.Round(adjustedCalories * 0.25m / 9m, MidpointRounding.AwayFromZero);
            int fatG         = Math.Max(fatFloorG, fatFromRatio);
            int fatKcal      = fatG * 9;

            // Carbs — fill remaining calories.
            int carbKcal = adjustedCalories - proteinKcal - fatKcal;
            int carbG    = Math.Max(0, (int)Math.Round((decimal)carbKcal / 4m, MidpointRounding.AwayFromZero));

            return (proteinG, carbG, fatG);
        }

        public TDEEResultDto ComputeFullResult(
            string              gender,
            decimal             weightKg,
            decimal             heightCm,
            DateTime?           dateOfBirth,
            int                 sessionsPerWeek,
            TrainingGoal        goal,
            decimal             proteinTargetPerKg,
            int                 fatFloorG,
            CalorieStrategyType strategyType,
            int?                absoluteTarget,
            int?                tdeeAdjustment)
        {
            int ageYears = dateOfBirth.HasValue
                ? CalculateAge(dateOfBirth.Value)
                : 30; // sensible fallback when DOB is unknown

            int bmr  = CalculateBMR(gender, weightKg, heightCm, ageYears);
            int tdee = ApplyActivityMultiplier(bmr, sessionsPerWeek);

            int adjustedCalories;
            string activityLabel = GetActivityLabel(sessionsPerWeek);

            if (strategyType == CalorieStrategyType.Absolute && absoluteTarget.HasValue)
            {
                adjustedCalories = absoluteTarget.Value;
            }
            else
            {
                int delta = tdeeAdjustment ?? _goalAdjustments.GetValueOrDefault(goal, 0);
                adjustedCalories = tdee + delta;
            }

            // Safety floor — never below 1200 kcal regardless of strategy.
            adjustedCalories = Math.Max(adjustedCalories, 1200);

            var (proteinG, carbG, fatG) = CalculateMacros(
                adjustedCalories, weightKg, proteinTargetPerKg, fatFloorG);

            return new TDEEResultDto
            {
                BMR               = bmr,
                TDEE              = tdee,
                AdjustedCalories  = adjustedCalories,
                ProteinTargetG    = proteinG,
                CarbTargetG       = carbG,
                FatTargetG        = fatG,
                Goal              = goal.ToString(),
                ActivityLevel     = activityLabel,
                CalculationMethod = "Mifflin-St Jeor (1990)"
            };
        }

        public int? ComputeEmpiricalTDEE(
            IReadOnlyList<WeeklyCheckIn> checkIns,
            int                         currentAdjustedKcal)
        {
            // Require at least 3 valid check-ins with submitted weights.
            var valid = checkIns
                .Where(c => c.AverageWeight > 0 && c.AdherencePercent >= 75)
                .OrderBy(c => c.WeekNumber)
                .ToList();

            if (valid.Count < 3) return null;

            // Weight delta over the validated window.
            decimal firstWeight = valid.First().AverageWeight;
            decimal lastWeight  = valid.Last().AverageWeight;
            decimal totalDelta  = lastWeight - firstWeight;   // negative = lost weight
            int     weekCount   = valid.Last().WeekNumber - valid.First().WeekNumber;

            if (weekCount < 1) return null;

            // 1 kg of body-mass ≈ 7,700 kcal (combined fat + glycogen/water model).
            // empiricalTDEE = calories consumed + (deficit represented by weight change)
            // If the trainee lost 0.5 kg/wk on 2200 kcal → TDEE ≈ 2200 + (0.5 × 7700 / 7)
            decimal weeklyDelta    = totalDelta / weekCount;
            decimal weeklyKcalDiff = weeklyDelta * 7700m / 7m;  // per-day kcal equivalent
            int     empiricalTDEE  = (int)Math.Round(currentAdjustedKcal - weeklyKcalDiff,
                                         MidpointRounding.AwayFromZero);

            // Sanity bounds — if the empirical value is wildly outside human range, discard.
            return (empiricalTDEE is >= 1000 and <= 8000) ? empiricalTDEE : (int?)null;
        }

        // ── Private helpers ─────────────────────────────────────────────────

        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow.Date;
            int age   = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return Math.Max(age, 16); // never compute a nonsensical age
        }

        private static string GetActivityLabel(int sessionsPerWeek) => sessionsPerWeek switch
        {
            0 or 1 => "Sedentary (0–1 sessions/week)",
            2      => "Lightly Active (2 sessions/week)",
            3 or 4 => "Moderately Active (3–4 sessions/week)",
            5 or 6 => "Very Active (5–6 sessions/week)",
            _      => "Extremely Active (7+ sessions/week)"
        };
    }
}
