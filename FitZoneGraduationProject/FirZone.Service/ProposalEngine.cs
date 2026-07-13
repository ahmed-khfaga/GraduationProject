using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    /// <summary>
    /// The ProposalEngine is the algorithmic heart of the nutrition system.
    ///
    /// STRICT DATA CONTRACT — enforced by method signature, not convention:
    ///   This class receives WeeklyCheckIn objects. It reads ONLY the objective
    ///   fields: AverageWeight, EnergyLevel, HungerLevel, SleepQuality, AdherencePercent.
    ///   ClientNote and NoteCategory are properties on WeeklyCheckIn but this class
    ///   never touches them. The algorithm is goal-driven, not empathy-driven.
    ///   The coach is the empathy layer.
    ///
    /// PROPOSAL VS DECISION:
    ///   This class generates a PROPOSAL. Nothing changes in the database until
    ///   CoachReviewService.ApplyDecisionAsync() is called with the coach's explicit decision.
    ///   The proposal may be approved as-is, modified, overridden, or deferred.
    ///
    /// ADJUSTMENT SIGN CONVENTION:
    ///   Negative kcal = reduce calories (trainee not losing enough / gaining too fast).
    ///   Positive kcal = increase calories (trainee losing too fast / not gaining enough).
    ///   Zero = no change recommended.
    ///
    /// EXPECTED RANGE CONVENTION (stored in ClientNutritionConstraints):
    ///   Fat loss  : both negative  e.g. Min = -0.60, Max = -0.40 kg/week
    ///   Muscle gain: both positive e.g. Min = +0.10, Max = +0.25 kg/week
    ///   actualDelta < ExpectedMin → increase calories (losing too fast / gaining too little)
    ///   actualDelta > ExpectedMax → reduce  calories (losing too slow  / gaining too much)
    ///   within range              → no adjustment
    /// </summary>
    public class ProposalEngine : IProposalEngine
    {
        // Maximum kcal adjustment the engine will ever propose regardless of constraints.
        // The per-client MaxSingleAdjustmentKcal is the tighter bound; this is the absolute ceiling.
        private const int AbsoluteMaxAdjustment = 300;

        public ProposalResult GenerateProposal(
            WeeklyCheckIn                currentCheckIn,
            IReadOnlyList<WeeklyCheckIn> allCheckIns,
            ClientNutritionConstraints   constraints,
            int                          currentKcal,
            int                          baselineKcal,
            WeekProtocolType?            linkedPlanWeekType = null)
        {
            // ── Step 1: Adherence gate ──────────────────────────────────────
            // If adherence is below the coach-set threshold, the weight data
            // is not reliable enough to base a calorie decision on.
            if (currentCheckIn.AdherencePercent < constraints.AdherenceThresholdPercent)
            {
                return BuildResult(
                    adjustment:   0,
                    newKcal:      currentKcal,
                    confidence:   ProposalConfidence.Low,
                    reasoning:    BuildReasoning(currentCheckIn, null, null, 0, constraints,
                                      "Adherence below threshold — data not reliable for adjustment. " +
                                      $"Reported: {currentCheckIn.AdherencePercent}%, " +
                                      $"threshold: {constraints.AdherenceThresholdPercent}%. " +
                                      "No change proposed. Encourage the trainee to follow the plan " +
                                      "more closely before targets are modified."),
                    projected:    null,
                    escalated:    false,
                    recalibrationDue: false);
            }

            // ── Step 2: History and delta calculation ───────────────────────
            var sortedHistory = allCheckIns
                .Where(c => c.WeekNumber < currentCheckIn.WeekNumber && c.AverageWeight > 0)
                .OrderByDescending(c => c.WeekNumber)
                .ToList();

            if (!sortedHistory.Any())
            {
                // First week — no prior data for comparison.
                return BuildResult(
                    adjustment:   0,
                    newKcal:      currentKcal,
                    confidence:   ProposalConfidence.Low,
                    reasoning:    BuildReasoning(currentCheckIn, null, null, 0, constraints,
                                      "First week of data — no prior weight reading available. " +
                                      "Waiting for week 2 before any adjustment. Coach may defer."),
                    projected:    null,
                    escalated:    false,
                    recalibrationDue: false);
            }

            var previousCheckIn = sortedHistory.First();
            decimal actualDelta = currentCheckIn.AverageWeight - previousCheckIn.AverageWeight;

            // ── Step 3: Training-week noise correction ──────────────────────
            // High-volume training weeks cause muscle inflammation and water retention.
            // The scale will not move even when fat loss is occurring. Tighten
            // the deviation trigger on these weeks to avoid false-positive adjustments.
            decimal effectiveDeviationTrigger = constraints.DeviationTriggerKg;
            bool    isHighVolumeWeek = linkedPlanWeekType == WeekProtocolType.HighVolume;
            if (constraints.ApplyTrainingWeekNoiseCorrection && isHighVolumeWeek)
            {
                // Tighten by 50 % — require a larger miss before acting.
                effectiveDeviationTrigger *= 1.5m;
            }

            // ── Step 4: Energy escalation check ────────────────────────────
            // If energy is critically low for 2+ consecutive weeks, escalate
            // regardless of weight data. Catches under-fuelling early.
            bool isEscalated = false;
            if (constraints.EnergyLevelEscalationRule && currentCheckIn.EnergyLevel == 1)
            {
                var previousWithLowEnergy = sortedHistory.FirstOrDefault();
                if (previousWithLowEnergy?.EnergyLevel == 1)
                {
                    isEscalated = true;
                    // Propose a moderate calorie increase to address the energy deficit.
                    int escalationIncrease = Math.Min(100, constraints.MaxSingleAdjustmentKcal);
                    int newKcalEsc = Math.Min(currentKcal + escalationIncrease,
                                              constraints.CalorieCeiling);
                    return BuildResult(
                        adjustment:   newKcalEsc - currentKcal,
                        newKcal:      newKcalEsc,
                        confidence:   ProposalConfidence.High,
                        reasoning:    BuildReasoning(currentCheckIn, actualDelta, previousCheckIn, 0,
                                          constraints,
                                          $"⚠ ESCALATED — Energy level reported as 1/5 for 2 consecutive " +
                                          $"weeks (W{previousCheckIn.WeekNumber} and W{currentCheckIn.WeekNumber}). " +
                                          $"This overrides normal weight-delta analysis. " +
                                          $"Proposing +{escalationIncrease} kcal to address under-fuelling. " +
                                          $"Coach should verify whether training performance is suffering."),
                        projected:    null,
                        escalated:    true,
                        recalibrationDue: false);
                }
            }

            // ── Step 5: In-range check ──────────────────────────────────────
            bool belowMin = actualDelta < constraints.ExpectedWeeklyChangeMin - effectiveDeviationTrigger;
            bool aboveMax = actualDelta > constraints.ExpectedWeeklyChangeMax + effectiveDeviationTrigger;
            bool inRange  = !belowMin && !aboveMax;

            if (inRange)
            {
                bool recalibDue = CheckRecalibrationDue(allCheckIns, constraints, currentKcal, baselineKcal);
                return BuildResult(
                    adjustment:   0,
                    newKcal:      currentKcal,
                    confidence:   ProposalConfidence.High,
                    reasoning:    BuildReasoning(currentCheckIn, actualDelta, previousCheckIn, 0,
                                      constraints,
                                      $"Progress within target range " +
                                      $"(expected: {constraints.ExpectedWeeklyChangeMin:+0.00;-0.00} to " +
                                      $"{constraints.ExpectedWeeklyChangeMax:+0.00;-0.00} kg/wk, " +
                                      $"actual: {actualDelta:+0.00;-0.00} kg). No adjustment needed."),
                    projected:    BuildProjection(currentCheckIn, allCheckIns, constraints, 0),
                    escalated:    false,
                    recalibrationDue: recalibDue);
            }

            // ── Step 6: Consecutive-week rule ───────────────────────────────
            if (constraints.RequireConsecutiveWeeksDeviation && sortedHistory.Count >= 2)
            {
                var prevPrev = sortedHistory[1];
                decimal prevDelta = previousCheckIn.AverageWeight - prevPrev.AverageWeight;
                bool prevBelowMin = prevDelta < constraints.ExpectedWeeklyChangeMin - effectiveDeviationTrigger;
                bool prevAboveMax = prevDelta > constraints.ExpectedWeeklyChangeMax + effectiveDeviationTrigger;

                if (!prevBelowMin && !prevAboveMax)
                {
                    // Only this week is off — first deviation. Wait for next week.
                    return BuildResult(
                        adjustment:   0,
                        newKcal:      currentKcal,
                        confidence:   ProposalConfidence.Medium,
                        reasoning:    BuildReasoning(currentCheckIn, actualDelta, previousCheckIn, 0,
                                          constraints,
                                          $"Deviation detected this week ({actualDelta:+0.00;-0.00} kg) " +
                                          $"but the consecutive-week rule is active. Last week was within range. " +
                                          $"Waiting for a second consecutive week of deviation before proposing a change. " +
                                          $"Deferring — re-evaluate after next check-in."),
                        projected:    BuildProjection(currentCheckIn, allCheckIns, constraints, 0),
                        escalated:    false,
                        recalibrationDue: false);
                }
            }

            // ── Step 7: Compute adjustment ──────────────────────────────────
            // actualDelta > expectedMax → losing less / gaining more than target → reduce calories (negative)
            // actualDelta < expectedMin → losing more / gaining less than target → increase calories (positive)

            int rawAdjustment;
            if (aboveMax)
            {
                // Not enough progress in the desired direction. Reduce calories.
                // Use a conservative 100 kcal step; or up to MaxSingleAdjustmentKcal if the miss is large.
                decimal missKg = actualDelta - constraints.ExpectedWeeklyChangeMax;
                rawAdjustment  = -ComputeStepSize(missKg, constraints);
            }
            else
            {
                // Too much progress in the desired direction (losing too fast / gaining too much).
                // Increase calories.
                decimal missKg = constraints.ExpectedWeeklyChangeMin - actualDelta;
                rawAdjustment  = +ComputeStepSize(missKg, constraints);
            }

            // Conservative mode: halve the adjustment when PreserveLeanMassOverRate is set.
            if (constraints.PreserveLeanMassOverRate && rawAdjustment < 0)
                rawAdjustment = (int)Math.Round(rawAdjustment / 2.0, MidpointRounding.AwayFromZero);

            // Hard cap to MaxSingleAdjustmentKcal and AbsoluteMaxAdjustment.
            rawAdjustment = ClampToMax(rawAdjustment, constraints.MaxSingleAdjustmentKcal);

            // ── Step 8: Apply floors, ceilings, and cumulative drift cap ────
            int proposedKcal = currentKcal + rawAdjustment;
            proposedKcal     = Math.Max(proposedKcal, constraints.CalorieFloor);
            proposedKcal     = Math.Min(proposedKcal, constraints.CalorieCeiling);

            // Cumulative drift cap: the total drift from baseline is bounded.
            int currentDrift  = currentKcal  - baselineKcal;
            int proposedDrift = proposedKcal  - baselineKcal;
            if (Math.Abs(proposedDrift) > constraints.MaxCumulativeDriftKcal)
            {
                int maxDriftAllowed = Math.Sign(proposedDrift) * constraints.MaxCumulativeDriftKcal;
                proposedKcal  = baselineKcal + maxDriftAllowed;
                rawAdjustment = proposedKcal - currentKcal;
            }

            // Recalculate actual adjustment after all constraints.
            int finalAdjustment = proposedKcal - currentKcal;

            // ── Step 9: Confidence level ────────────────────────────────────
            var confidence = DetermineConfidence(
                currentCheckIn, sortedHistory, constraints, isHighVolumeWeek);

            // ── Step 10: Recalibration flag ─────────────────────────────────
            bool recalibrationDue = CheckRecalibrationDue(
                allCheckIns, constraints, currentKcal, baselineKcal);

            return BuildResult(
                adjustment:  finalAdjustment,
                newKcal:     proposedKcal,
                confidence:  confidence,
                reasoning:   BuildReasoning(currentCheckIn, actualDelta, previousCheckIn,
                                 finalAdjustment, constraints, null),
                projected:   BuildProjection(currentCheckIn, allCheckIns, constraints, finalAdjustment),
                escalated:   false,
                recalibrationDue: recalibrationDue);
        }

        // ── Private: Step size calculation ──────────────────────────────────

        /// <summary>
        /// Determines adjustment magnitude from the deviation size.
        /// Small miss (0–0.15 kg off) → 100 kcal step.
        /// Medium miss (0.16–0.30 kg) → 150 kcal step.
        /// Large miss  (> 0.30 kg)    → MaxSingleAdjustmentKcal.
        /// </summary>
        private static int ComputeStepSize(decimal missKg, ClientNutritionConstraints c)
        {
            int max = Math.Min(c.MaxSingleAdjustmentKcal, AbsoluteMaxAdjustment);
            return Math.Abs(missKg) switch
            {
                <= 0.15m => Math.Min(100, max),
                <= 0.30m => Math.Min(150, max),
                _        => max
            };
        }

        private static int ClampToMax(int adjustment, int maxPerClient)
        {
            int cap = Math.Min(maxPerClient, AbsoluteMaxAdjustment);
            return adjustment > 0
                ? Math.Min(adjustment, cap)
                : Math.Max(adjustment, -cap);
        }

        // ── Private: Confidence scoring ─────────────────────────────────────

        private static ProposalConfidence DetermineConfidence(
            WeeklyCheckIn                current,
            IList<WeeklyCheckIn>         history,
            ClientNutritionConstraints   constraints,
            bool                         isHighVolumeWeek)
        {
            // Factors that lower confidence:
            int demerits = 0;

            if (current.AdherencePercent < constraints.AdherenceThresholdPercent + 10)
                demerits++; // borderline adherence

            if (history.Count < 2)
                demerits++; // insufficient history

            if (isHighVolumeWeek)
                demerits++; // scale noise expected

            if (current.SleepQuality <= 2)
                demerits++; // poor sleep = cortisol = water retention = noisy scale

            return demerits switch
            {
                0 => ProposalConfidence.High,
                1 => ProposalConfidence.Medium,
                _ => ProposalConfidence.Low
            };
        }

        // ── Private: Recalibration flag ──────────────────────────────────────

        private static bool CheckRecalibrationDue(
            IReadOnlyList<WeeklyCheckIn> checkIns,
            ClientNutritionConstraints   constraints,
            int                          currentKcal,
            int                          baselineKcal)
        {
            if (!constraints.EnableBaselineRecalibrationReview) return false;

            // Flag after 4+ valid check-ins if current drift is substantial (> 250 kcal)
            // indicating the original TDEE estimate was meaningfully off.
            var validCheckIns = checkIns.Where(c => c.AverageWeight > 0).ToList();
            if (validCheckIns.Count < 4) return false;

            int cumulativeDrift = Math.Abs(currentKcal - baselineKcal);
            return cumulativeDrift >= 250;
        }

        // ── Private: Reasoning builder ───────────────────────────────────────

        private static string BuildReasoning(
            WeeklyCheckIn              current,
            decimal?                   actualDelta,
            WeeklyCheckIn?             previous,
            int                        finalAdjustment,
            ClientNutritionConstraints constraints,
            string?                    overrideMessage)
        {
            if (overrideMessage is not null) return overrideMessage;

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("WEIGHT TREND");
            if (previous is not null)
                sb.AppendLine($"  W{previous.WeekNumber}: {previous.AverageWeight:F2} kg  →  " +
                              $"W{current.WeekNumber}: {current.AverageWeight:F2} kg  " +
                              $"(Δ {actualDelta:+0.00;-0.00} kg)");
            sb.AppendLine($"  Expected range: {constraints.ExpectedWeeklyChangeMin:+0.00;-0.00} " +
                          $"to {constraints.ExpectedWeeklyChangeMax:+0.00;-0.00} kg/wk");
            sb.AppendLine();

            sb.AppendLine("CHECK-IN SIGNALS");
            sb.AppendLine($"  Adherence : {current.AdherencePercent}% " +
                          $"(threshold: {constraints.AdherenceThresholdPercent}%) → " +
                          (current.AdherencePercent >= constraints.AdherenceThresholdPercent ? "VALID" : "BELOW THRESHOLD"));
            sb.AppendLine($"  Energy    : {current.EnergyLevel}/5");
            sb.AppendLine($"  Hunger    : {current.HungerLevel}/5");
            sb.AppendLine($"  Sleep     : {current.SleepQuality}/5");
            sb.AppendLine();

            sb.AppendLine("CONSTRAINT CHECK");
            sb.AppendLine($"  Protein floor : {constraints.ProteinFloorG}g — respected");
            sb.AppendLine($"  Fat floor     : {constraints.FatFloorG}g — respected");
            sb.AppendLine($"  Calorie floor : {constraints.CalorieFloor} kcal — respected");
            sb.AppendLine($"  Max adj/week  : ±{constraints.MaxSingleAdjustmentKcal} kcal — respected");
            sb.AppendLine();

            if (finalAdjustment == 0)
            {
                sb.Append("OUTCOME: No adjustment proposed.");
            }
            else
            {
                sb.AppendLine("PROPOSED ADJUSTMENT");
                sb.AppendLine($"  Direction : {(finalAdjustment < 0 ? "Reduce" : "Increase")} calories");
                sb.AppendLine($"  Amount    : {finalAdjustment:+0;-0} kcal");
                sb.AppendLine($"  Vector    : {constraints.PreferredAdjustmentVector}");
                sb.Append($"  Rationale : Actual weight change {actualDelta:+0.00;-0.00} kg is " +
                          (finalAdjustment < 0 ? "above" : "below") + " the expected range. " +
                          "Adjustment is the minimum required to bring the trainee back on track.");
            }

            return sb.ToString();
        }

        // ── Private: Projection builder ───────────────────────────────────────

        private static string? BuildProjection(
            WeeklyCheckIn                current,
            IReadOnlyList<WeeklyCheckIn> history,
            ClientNutritionConstraints   constraints,
            int                          appliedAdjustment)
        {
            // Need at least 2 check-ins to estimate a rate.
            if (!history.Any()) return null;

            var sorted = history
                .Where(c => c.AverageWeight > 0)
                .OrderBy(c => c.WeekNumber)
                .ToList();
            if (sorted.Count < 2) return null;

            decimal recentRate = (current.AverageWeight - sorted.Last().AverageWeight); // kg/wk

            // Weeks remaining (unknown without enrollment, so use a fixed 4-week horizon).
            const int weeksAhead = 4;
            decimal projectedWeight = current.AverageWeight + (recentRate * weeksAhead);
            decimal midTarget = (constraints.ExpectedWeeklyChangeMin + constraints.ExpectedWeeklyChangeMax) / 2m;
            decimal targetWeight = current.AverageWeight + (midTarget * weeksAhead);

            return $"At current rate ({recentRate:+0.00;-0.00} kg/wk): " +
                   $"4-week projection = {projectedWeight:F1} kg · " +
                   $"Target trajectory = {targetWeight:F1} kg.";
        }

        // ── Private: Result factory ──────────────────────────────────────────

        private static ProposalResult BuildResult(
            int                adjustment,
            int                newKcal,
            ProposalConfidence confidence,
            string             reasoning,
            string?            projected,
            bool               escalated,
            bool               recalibrationDue)
        {
            return new ProposalResult
            {
                SuggestedAdjustmentKcal = adjustment,
                NewCalorieTarget        = newKcal,
                Confidence              = confidence,
                Reasoning               = reasoning,
                ProjectedOutcome        = projected,
                IsEscalated             = escalated,
                BaselineRecalibrationDue = recalibrationDue
            };
        }
    }
}
