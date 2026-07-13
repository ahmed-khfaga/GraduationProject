using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Core.Specifications.CommandSpec.SessionSpec;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    /// <summary>
    /// Manages the full check-in lifecycle:
    ///   1. Trainee submits check-in (objective data + optional note).
    ///   2. ProposalEngine generates a proposal from objective data ONLY.
    ///   3. Coach reviews and records a decision.
    ///   4. Decision is applied: calorie targets updated, week N+1 unlocked.
    ///
    /// DATA SEPARATION CONTRACT:
    ///   SubmitAsync passes ONLY objective fields to ProposalEngine.
    ///   ClientNote and NoteCategory are stored on the WeeklyCheckIn record
    ///   but are never passed to any calculation method. This is enforced
    ///   by the IProposalEngine.GenerateProposal signature — it has no note parameter.
    ///
    /// MACRO ADJUSTMENT:
    ///   ApplyDecisionAsync applies the calorie change to the live DayProtocol
    ///   targets inside the enrollment's current week, following the
    ///   AppliedAdjustmentVector set by the coach. Protein floor is always respected.
    /// </summary>
    public class CheckInService : ICheckInService
    {
        private readonly IUnitOfWork _uow;
        private readonly IProposalEngine _engine;
        private readonly ITDEEService _tdeeService;
        private readonly NutritionEnrollmentService _enrollmentService;

        public CheckInService(
            IUnitOfWork uow,
            IProposalEngine engine,
            ITDEEService tdeeService,
            NutritionEnrollmentService enrollmentService)
        {
            _uow = uow;
            _engine = engine;
            _tdeeService = tdeeService;
            _enrollmentService = enrollmentService;
        }

        // ── 1. Trainee submits check-in ─────────────────────────────────────

        public async Task<CheckInConfirmationDto> SubmitAsync(
            int               enrollmentId,
            int               traineeId,
            SubmitCheckInDto  dto)
        {
            // Verify enrollment belongs to this trainee.
            var enrollSpec  = new NutritionEnrollmentByIdSpec(enrollmentId, traineeId);
            var enrollment  = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetWithSpecAsync(enrollSpec)
                             ?? throw new InvalidOperationException("Enrollment not found.");

            if (!enrollment.IsActive)
                throw new InvalidOperationException(
                    "Cannot submit a check-in for an inactive enrollment.");

            int weekNumber = enrollment.MaxWeekUnlocked;

            // Prevent duplicate check-in for the same week.
            var existingSpec = new CheckInByWeekSpec(enrollmentId, weekNumber);
            var existing     = await _uow.Repository<WeeklyCheckIn>().GetWithSpecAsync(existingSpec);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A check-in for Week {weekNumber} has already been submitted.");

            // Calculate average weight from submitted readings.
            decimal avgWeight = CalculateAverageWeight(
                dto.MorningWeight1, dto.MorningWeight2, dto.MorningWeight3);

            // ── Build check-in record ────────────────────────────────────────
            // ClientNote is stored here — it goes NO further (not to ProposalEngine).
            var checkIn = new WeeklyCheckIn
            {
                EnrollmentID     = enrollmentId,
                WeekNumber       = weekNumber,
                SubmittedAt      = DateTime.UtcNow,
                MorningWeight1   = dto.MorningWeight1,
                MorningWeight2   = dto.MorningWeight2,
                MorningWeight3   = dto.MorningWeight3,
                AverageWeight    = avgWeight,
                EnergyLevel      = dto.EnergyLevel,
                HungerLevel      = dto.HungerLevel,
                SleepQuality     = dto.SleepQuality,
                AdherencePercent = dto.AdherencePercent,
                ClientNote       = dto.ClientNote,    // stored — NOT passed to engine
                NoteCategory     = dto.NoteCategory   // stored — NOT passed to engine
            };

            // ── Run proposal engine on OBJECTIVE data only ───────────────────
            var constraints = enrollment.Constraints;
            if (constraints is not null)
            {
                var historySpec = new AllCheckInsForEnrollmentSpec(enrollmentId);
                var history     = (await _uow.Repository<WeeklyCheckIn>()
                                             .GetAllWithSpecAsync(historySpec))
                                  .ToList();

                // Determine linked training week type for noise correction.
                WeekProtocolType? linkedWeekType = await GetLinkedWeekTypeAsync(
                    enrollment, weekNumber);

                // ENFORCE DATA SEPARATION: ProposalEngine receives no note fields.
                var proposal = _engine.GenerateProposal(
                    currentCheckIn:   checkIn,
                    allCheckIns:      history,
                    constraints:      constraints,
                    currentKcal:      enrollment.CurrentAdjustedKcal,
                    baselineKcal:     enrollment.BaselineCalories,
                    linkedPlanWeekType: linkedWeekType);

                checkIn.SystemProposalKcal        = proposal.SuggestedAdjustmentKcal;
                checkIn.SystemProposalReasoning   = proposal.Reasoning;
                checkIn.SystemConfidence          = proposal.Confidence;
                checkIn.ProjectedOutcomeIfNoAction = proposal.ProjectedOutcome;

                // Update empirical TDEE on the enrollment if recalibration is due.
                if (proposal.BaselineRecalibrationDue)
                {
                    var allCi         = history.Append(checkIn).ToList();
                    int? empiricalTDEE = _engine is ProposalEngine
                        ? null  // computed separately via TDEEService
                        : null;
                    // TDEEService.ComputeEmpiricalTDEE is called via CoachReviewService
                    // after the coach approves, keeping this method simple.
                }
            }

            _uow.Repository<WeeklyCheckIn>().Add(checkIn);
            await _uow.CompleteAsync();

            return new CheckInConfirmationDto
            {
                CheckInId          = checkIn.Id,
                WeekNumber         = weekNumber,
                AverageWeight      = avgWeight,
                Message            = "Check-in received. Your coach will review within 48 hours.",
                CoachReviewPending = true
            };
        }

        // ── 2. Coach reads the proposal ─────────────────────────────────────

        public async Task<CheckInProposalDto?> GetProposalAsync(int checkInId, int coachId)
        {
            var checkIn = await _uow.Repository<WeeklyCheckIn>().GetAsync(checkInId);
            if (checkIn is null) return null;

            // Verify coach owns the plan of this enrollment.
            var enrollSpec  = new NutritionEnrollmentByIdForCoachSpec(checkIn.EnrollmentID, coachId);
            var enrollment  = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetWithSpecAsync(enrollSpec);
            if (enrollment is null) return null;

            // Load full history for the weight graph.
            var historySpec = new AllCheckInsForEnrollmentSpec(checkIn.EnrollmentID);
            var history     = (await _uow.Repository<WeeklyCheckIn>()
                                         .GetAllWithSpecAsync(historySpec))
                              .OrderBy(c => c.WeekNumber)
                              .ToList();

            // Previous check-in for delta calculation.
            var prev = history
                .Where(c => c.WeekNumber < checkIn.WeekNumber && c.AverageWeight > 0)
                .OrderByDescending(c => c.WeekNumber)
                .FirstOrDefault();

            decimal delta = prev is not null ? checkIn.AverageWeight - prev.AverageWeight : 0m;

            var constraints = enrollment.Constraints;

            var trainee = await _uow.Repository<Trainee>().GetAsync(enrollment.TraineeID);

            return new CheckInProposalDto
            {
                CheckInId     = checkIn.Id,
                EnrollmentId  = checkIn.EnrollmentID,
                TraineeName   = trainee?.ApplicationUser?.FullName ?? string.Empty,
                WeekNumber    = checkIn.WeekNumber,
                AverageWeight = checkIn.AverageWeight,
                WeightDeltaKg = delta,
                ExpectedMin   = constraints?.ExpectedWeeklyChangeMin ?? 0m,
                ExpectedMax   = constraints?.ExpectedWeeklyChangeMax ?? 0m,
                EnergyLevel      = checkIn.EnergyLevel,
                HungerLevel      = checkIn.HungerLevel,
                SleepQuality     = checkIn.SleepQuality,
                AdherencePercent = checkIn.AdherencePercent,

                // ClientNote is shown to the coach separately from algorithm output.
                ClientNote   = checkIn.ClientNote,
                NoteCategory = checkIn.NoteCategory?.ToString(),

                SystemProposalKcal        = checkIn.SystemProposalKcal,
                SystemProposalReasoning   = checkIn.SystemProposalReasoning ?? string.Empty,
                SystemConfidence          = checkIn.SystemConfidence.ToString(),
                ProjectedOutcomeIfNoAction = checkIn.ProjectedOutcomeIfNoAction,

                WeightHistory = history
                    .Where(c => c.AverageWeight > 0)
                    .Select(c => new WeightHistoryPointDto
                    {
                        WeekNumber      = c.WeekNumber,
                        AverageWeight   = c.AverageWeight,
                        CaloriesApplied = c.FinalAdjustmentKcal.HasValue
                            ? enrollment.BaselineCalories + c.FinalAdjustmentKcal
                            : enrollment.CurrentAdjustedKcal
                    }).ToList()
            };
        }

        // ── 3. Coach applies decision ────────────────────────────────────────

        public async Task ApplyDecisionAsync(
            int                    checkInId,
            int                    coachId,
            CoachCheckInDecisionDto dto)
        {
            var checkIn = await _uow.Repository<WeeklyCheckIn>().GetAsync(checkInId);
            if (checkIn is null)
                throw new InvalidOperationException("Check-in not found.");

            if (checkIn.CoachApprovedAt.HasValue)
                throw new InvalidOperationException("This check-in has already been reviewed.");

            // Verify coach owns the plan.
            var enrollSpec  = new NutritionEnrollmentByIdForCoachSpec(checkIn.EnrollmentID, coachId);
            var enrollment  = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetWithSpecAsync(enrollSpec)
                             ?? throw new InvalidOperationException(
                                    "Enrollment not found or access denied.");

            // ── Record the decision ──────────────────────────────────────────
            checkIn.CoachDecision           = dto.Decision;
            checkIn.CoachNote               = dto.CoachNote;
            checkIn.CoachNoteAction         = dto.NoteAction;
            checkIn.CoachReviewedAt         = DateTime.UtcNow;
            checkIn.CoachApprovedAt         = DateTime.UtcNow;  // unlocks week N+1

            int finalAdjustment = 0;

            if (dto.Decision != CoachDecisionType.Deferred)
            {
                finalAdjustment = dto.Decision switch
                {
                    CoachDecisionType.Approved  => checkIn.SystemProposalKcal ?? 0,
                    CoachDecisionType.Modified  => dto.FinalAdjustmentKcal ?? 0,
                    CoachDecisionType.Override  => dto.FinalAdjustmentKcal ?? 0,
                    _                           => 0
                };

                checkIn.FinalAdjustmentKcal      = finalAdjustment;
                checkIn.AppliedAdjustmentVector  = dto.AppliedAdjustmentVector
                                                   ?? enrollment.Constraints?.PreferredAdjustmentVector
                                                   ?? AdjustmentVector.RestDayCarbs;

                // ── Apply calorie change to live enrollment target ────────────
                int newKcal = enrollment.CurrentAdjustedKcal + finalAdjustment;

                // Respect hard floors and ceilings from constraints.
                var c = enrollment.Constraints;
                if (c is not null)
                {
                    newKcal = Math.Max(newKcal, c.CalorieFloor);
                    newKcal = Math.Min(newKcal, c.CalorieCeiling);
                }

                enrollment.CurrentAdjustedKcal = newKcal;

                // ── Propagate adjustment to DayProtocols for week N+1 ────────
                if (finalAdjustment != 0)
                {
                    await ApplyMacroAdjustmentAsync(
                        enrollment,
                        enrollment.NutritionPlan.DurationOnWeeks,
                        checkIn.WeekNumber + 1,
                        finalAdjustment,
                        checkIn.AppliedAdjustmentVector ?? AdjustmentVector.RestDayCarbs,
                        c);
                }

                // ── Empirical TDEE recalibration flag ────────────────────────
                // Surface to the enrollment record so the coach can review baseline.
                // Compute and persist empirical TDEE from real weight-change data.
                // Runs only when EnableBaselineRecalibrationReview = true (set by coach in constraints).
                if (c?.EnableBaselineRecalibrationReview == true)
                {
                    var allCiSpec = new AllCheckInsForEnrollmentSpec(enrollment.Id);
                    var allCi = (await _uow.Repository<WeeklyCheckIn>()
                                               .GetAllWithSpecAsync(allCiSpec))
                                    .ToList();

                    // Include the check-in we are approving now (not yet saved to DB).
                    allCi.Add(checkIn);

                    int? empiricalTDEE = _tdeeService.ComputeEmpiricalTDEE(
                        allCi.AsReadOnly(),
                        enrollment.CurrentAdjustedKcal);

                    if (empiricalTDEE.HasValue)
                        enrollment.EmpiricalTDEEKcal = empiricalTDEE.Value;
                }
            }

            _uow.Repository<WeeklyCheckIn>().Update(checkIn);
            _uow.Repository<TraineeNutritionEnrollment>().Update(enrollment);
            await _uow.CompleteAsync();

            // Sync MaxWeekUnlocked now that CoachApprovedAt is set.
            await _enrollmentService.SyncMaxWeekUnlockedAsync(enrollment);
        }

        // ── 4. Coach manually unlocks (missed check-in) ──────────────────────

        public async Task ManuallyApproveUnlockAsync(int enrollmentId, int coachId)
        {
            var enrollSpec  = new NutritionEnrollmentByIdForCoachSpec(enrollmentId, coachId);
            var enrollment  = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetWithSpecAsync(enrollSpec)
                             ?? throw new InvalidOperationException(
                                    "Enrollment not found or access denied.");

            int weekNumber = enrollment.MaxWeekUnlocked;

            // Check if a check-in already exists (coach should use ApplyDecisionAsync instead).
            var ciSpec   = new CheckInByWeekSpec(enrollmentId, weekNumber);
            var existing = await _uow.Repository<WeeklyCheckIn>().GetWithSpecAsync(ciSpec);
            if (existing is not null)
                throw new InvalidOperationException(
                    "A check-in exists for this week. Use the review panel to record your decision.");

            // Create a minimal placeholder check-in record so the approval gate can be set.
            var placeholder = new WeeklyCheckIn
            {
                EnrollmentID     = enrollmentId,
                WeekNumber       = weekNumber,
                SubmittedAt      = DateTime.UtcNow,
                MorningWeight1   = 0m,   // no data — coach manually approved
                AverageWeight    = 0m,
                EnergyLevel      = 3,
                HungerLevel      = 3,
                SleepQuality     = 3,
                AdherencePercent = 0,
                CoachDecision    = CoachDecisionType.Deferred,
                CoachNote        = "Week manually unlocked by coach — no check-in submitted.",
                CoachReviewedAt  = DateTime.UtcNow,
                CoachApprovedAt  = DateTime.UtcNow
            };

            _uow.Repository<WeeklyCheckIn>().Add(placeholder);
            await _uow.CompleteAsync();

            await _enrollmentService.SyncMaxWeekUnlockedAsync(enrollment);
        }
        public async Task<IEnumerable<TraineeCheckInHistoryDto>> GetTraineeHistoryAsync(
    int enrollmentId, int traineeId)
        {
            var enrollSpec = new NutritionEnrollmentByIdSpec(enrollmentId, traineeId);
            var enrollment = await _uow.Repository<TraineeNutritionEnrollment>()
                                       .GetWithSpecAsync(enrollSpec)
                          ?? throw new InvalidOperationException("Enrollment not found.");

            var historySpec = new AllCheckInsForEnrollmentSpec(enrollmentId);
            var checkIns = (await _uow.Repository<WeeklyCheckIn>()
                                         .GetAllWithSpecAsync(historySpec))
                              .OrderBy(c => c.WeekNumber)
                              .ToList();

            return checkIns.Select(c => new TraineeCheckInHistoryDto
            {
                WeekNumber = c.WeekNumber,
                SubmittedAt = c.SubmittedAt,
                AverageWeight = c.AverageWeight > 0 ? c.AverageWeight : null,
                EnergyLevel = c.EnergyLevel,
                HungerLevel = c.HungerLevel,
                SleepQuality = c.SleepQuality,
                AdherencePercent = c.AdherencePercent,
                CaloriesApplied = c.FinalAdjustmentKcal.HasValue
                    ? enrollment.BaselineCalories + c.FinalAdjustmentKcal.Value
                    : enrollment.CurrentAdjustedKcal,
                AdjustmentKcal = c.FinalAdjustmentKcal,
                CoachDirectiveNote = c.CoachNote,
                CoachReviewed = c.CoachApprovedAt.HasValue
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRIVATE: Macro adjustment propagation
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Applies the calorie adjustment to the DayProtocol records for the target week.
        ///
        /// Vector semantics:
        ///   RestDayCarbs     — only RestDay protocols absorb the change via CarbTargetG.
        ///   TrainingDayCarbs — only TrainingDay protocols absorb the change via CarbTargetG.
        ///   Fat              — all protocols absorb via FatTargetG (floor enforced).
        ///   Proportional     — all protocols split 60% carbs / 40% fat.
        ///
        /// Protein is NEVER modified.
        /// TotalCaloriesTarget is recalculated from actual macros after each update.
        /// </summary>
        private async Task ApplyMacroAdjustmentAsync(
            TraineeNutritionEnrollment enrollment,
            int totalWeeks,
            int targetWeekNumber,
            int adjustmentKcal,
            AdjustmentVector vector,
            ClientNutritionConstraints? constraints)
        {
            if (targetWeekNumber > totalWeeks) return;

            var weekSpec = new NutritionWeekFullDetailSpec(
                enrollment.NutritionPlanID, targetWeekNumber);
            var week = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(weekSpec);
            if (week is null) return;

            var protocolsToAdjust = vector switch
            {
                AdjustmentVector.RestDayCarbs => week.DayProtocols
                    .Where(d => d.DayProtocolType == DayProtocolType.RestDay).ToList(),
                AdjustmentVector.TrainingDayCarbs => week.DayProtocols
                    .Where(d => d.DayProtocolType == DayProtocolType.TrainingDay).ToList(),
                _ => week.DayProtocols.ToList()
            };

            // Fall back to all protocols if the targeted type doesn't exist in this week.
            if (!protocolsToAdjust.Any())
                protocolsToAdjust = week.DayProtocols.ToList();

            if (!protocolsToAdjust.Any()) return;

            // Distribute the total adjustment evenly across selected protocols.
            int adjustPerProtocol = (int)Math.Round(
                (decimal)adjustmentKcal / protocolsToAdjust.Count,
                MidpointRounding.AwayFromZero);

            int fatFloor = constraints?.FatFloorG ?? 50;

            foreach (var protocol in protocolsToAdjust)
            {
                switch (vector)
                {
                    case AdjustmentVector.RestDayCarbs:
                    case AdjustmentVector.TrainingDayCarbs:
                        {
                            // 1g carb = 4 kcal
                            int carbDeltaG = (int)Math.Round(
                                (decimal)adjustPerProtocol / 4m,
                                MidpointRounding.AwayFromZero);
                            protocol.CarbTargetG = Math.Max(0, protocol.CarbTargetG + carbDeltaG);
                            break;
                        }
                    case AdjustmentVector.Fat:
                        {
                            // 1g fat = 9 kcal
                            int fatDeltaG = (int)Math.Round(
                                (decimal)adjustPerProtocol / 9m,
                                MidpointRounding.AwayFromZero);
                            protocol.FatTargetG = Math.Max(fatFloor, protocol.FatTargetG + fatDeltaG);
                            break;
                        }
                    case AdjustmentVector.Proportional:
                        {
                            // 60% carbs, 40% fat
                            int carbDeltaG = (int)Math.Round(
                                adjustPerProtocol * 0.6m / 4m, MidpointRounding.AwayFromZero);
                            int fatDeltaG = (int)Math.Round(
                                adjustPerProtocol * 0.4m / 9m, MidpointRounding.AwayFromZero);
                            protocol.CarbTargetG = Math.Max(0, protocol.CarbTargetG + carbDeltaG);
                            protocol.FatTargetG = Math.Max(fatFloor, protocol.FatTargetG + fatDeltaG);
                            break;
                        }
                }

                // Recalculate TotalCaloriesTarget from actual macros so it is always consistent.
                // Formula: Protein × 4  +  Carbs × 4  +  Fat × 9
                protocol.TotalCaloriesTarget =
                    (protocol.ProteinTargetG * 4) +
                    (protocol.CarbTargetG * 4) +
                    (protocol.FatTargetG * 9);

                _uow.Repository<DayProtocol>().Update(protocol);
            }

            await _uow.CompleteAsync();
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static decimal CalculateAverageWeight(
            decimal w1, decimal? w2, decimal? w3)
        {
            var readings = new List<decimal> { w1 };
            if (w2.HasValue && w2.Value > 0) readings.Add(w2.Value);
            if (w3.HasValue && w3.Value > 0) readings.Add(w3.Value);
            return Math.Round(readings.Average(), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Resolves the WeekProtocolType of the linked training program week.
        /// Used by the ProposalEngine for high-volume week noise correction.
        /// Returns null if not a bundle enrollment or if no training week exists.
        /// </summary>
        private async Task<WeekProtocolType?> GetLinkedWeekTypeAsync(
            TraineeNutritionEnrollment enrollment, int weekNumber)
        {
            if (!enrollment.LinkedWorkoutEnrollmentID.HasValue)
                return null;

            var trainingEnroll = await _uow.Repository<TraineeProgramEnrollment>()
                                           .GetAsync(enrollment.LinkedWorkoutEnrollmentID.Value);
            if (trainingEnroll is null)
                return null;

            var weekSpec = new ProgramWeekByNumberSpec(
                trainingEnroll.WorkoutProgramId, weekNumber);
            var programWeek = await _uow.Repository<ProgramWeek>()
                                        .GetWithSpecAsync(weekSpec);

            if (programWeek is null)
                return null;

            // Every 4th week is treated as a Deload week.
            if (weekNumber % 4 == 0)
                return WeekProtocolType.Deload;

            // Weeks with 5 or more sessions are treated as HighVolume (noise correction fires).
            bool isHighVolume = programWeek.WorkoutSessions?.Count >= 5;
            return isHighVolume ? WeekProtocolType.HighVolume : WeekProtocolType.Standard;
        }
    }
}
