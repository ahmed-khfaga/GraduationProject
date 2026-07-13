using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    /// <summary>
    /// Manages the full lifecycle of a TraineeNutritionEnrollment.
    ///
    /// WEEK UNLOCK — dual gate (both must pass before MaxWeekUnlocked advances):
    ///   Gate 1 — Time: 7 Monday-anchored calendar days per week (same as training system).
    ///   Gate 2 — Coach: WeeklyCheckIn.CoachApprovedAt must be set for week N before
    ///            week N+1 becomes accessible.
    ///
    ///   Algorithm:
    ///     maxByTime  = min(floor((nowMonday – startMonday).Days / 7) + 1, totalWeeks)
    ///     maxByApproval starts at 1 (week 1 never requires prior approval).
    ///     For weekNum = 1 to maxByTime − 1:
    ///       if checkIn for weekNum exists and CoachApprovedAt is set → maxByApproval = weekNum + 1
    ///       else → break (cannot jump past an unapproved week)
    ///     MaxWeekUnlocked = min(maxByTime, maxByApproval, totalWeeks)
    ///
    /// RESUME LOGIC:
    ///   Mirrors TraineeProgramEnrollment resume pattern exactly.
    ///   PreviousNutritionEnrollmentByPlanSpec finds the inactive row with the highest
    ///   MaxWeekUnlocked. BackdateStartDate restores the StartDate so the time gate
    ///   reflects the saved progress without requiring the coach to re-approve old weeks.
    /// </summary>
    public class NutritionEnrollmentService : INutritionEnrollmentService
    {
        private readonly IUnitOfWork    _uow;
        private readonly IMapper        _mapper;
        private readonly ITDEEService   _tdee;
        private readonly ConstraintService _constraints;

        public NutritionEnrollmentService(
            IUnitOfWork         uow,
            IMapper             mapper,
            ITDEEService        tdee,
            ConstraintService constraints)
        {
            _uow         = uow;
            _mapper      = mapper;
            _tdee        = tdee;
            _constraints = constraints;
        }

        // ── Dashboard ────────────────────────────────────────────────────────

        public async Task<IEnumerable<NutritionEnrollmentDto>> GetMyEnrollmentsAsync(int traineeId)
        {
            var spec        = new TraineeActiveNutritionEnrollmentsSpec(traineeId);
            var enrollments = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetAllWithSpecAsync(spec);

            var result = new List<NutritionEnrollmentDto>();
            foreach (var e in enrollments)
            {
                await SyncMaxWeekUnlockedAsync(e);
                result.Add(await MapToEnrollmentDtoAsync(e));
            }
            return result;
        }

        public async Task<IEnumerable<NutritionEnrollmentHistoryDto>> GetMyEnrollmentHistoryAsync(
            int traineeId)
        {
            var spec        = new TraineeAllNutritionEnrollmentsSpec(traineeId);
            var enrollments = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetAllWithSpecAsync(spec);

            return enrollments.Select(e => new NutritionEnrollmentHistoryDto
            {
                Id                  = e.Id,
                NutritionPlanID     = e.NutritionPlanID,
                PlanName            = e.NutritionPlan?.Name ?? string.Empty,
                CoachName           = e.NutritionPlan?.Coach?.ApplicationUser?.FullName ?? string.Empty,
                MaxWeekUnlocked     = e.MaxWeekUnlocked,
                TotalWeeks          = e.NutritionPlan?.DurationOnWeeks ?? 0,
                Status              = e.Status.ToString(),
                StartDate           = e.StartDate,
                EndDate             = e.EndDate,
                BaselineCalories    = e.BaselineCalories,
                CurrentAdjustedKcal = e.CurrentAdjustedKcal,
                PendingCheckIn      = false,
                PendingCoachReview  = false,
                IsActive            = e.IsActive
            });
        }

        // ── Week access ──────────────────────────────────────────────────────

        public async Task<NutritionWeekDetailDto?> GetWeekAsync(
            int enrollmentId, int weekNumber, int traineeId)
        {
            var enrollSpec  = new NutritionEnrollmentByIdSpec(enrollmentId, traineeId);
            var enrollment  = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetWithSpecAsync(enrollSpec);
            if (enrollment is null) return null;

            await SyncMaxWeekUnlockedAsync(enrollment);

            if (weekNumber > enrollment.MaxWeekUnlocked) return null;  // locked → 404

            var weekSpec = new NutritionWeekFullDetailSpec(enrollment.NutritionPlanID, weekNumber);
            var week     = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(weekSpec);
            if (week is null) return null;

            // Fetch the coach note from the check-in of the PREVIOUS week (if any).
            string? coachNote = null;
            if (weekNumber > 1)
            {
                var prevCiSpec = new CheckInByWeekSpec(enrollmentId, weekNumber - 1);
                var prevCi     = await _uow.Repository<WeeklyCheckIn>().GetWithSpecAsync(prevCiSpec);
                coachNote      = prevCi?.CoachNote;
            }

            return new NutritionWeekDetailDto
            {
                WeekNumber         = week.WeekNumber,
                WeekProtocolType   = week.WeekProtocolType.ToString(),
                WeekDescription    = week.WeekDescription,
                FocusNote          = week.FocusNote,
                ProgressionNote    = week.ProgressionNote,
                NextWeekPreview    = week.NextWeekPreview,
                IsUnlocked         = true,
                CoachDirectiveNote = coachNote,
                DayProtocols       = _mapper.Map<List<DayProtocolDto>>(
                    week.DayProtocols.OrderBy(d => d.WeekDay).ThenBy(d => d.DayOrder))
            };
        }

        // ── Enrol or resume ──────────────────────────────────────────────────

        public async Task<NutritionEnrollmentDto> StartEnrollmentAsync(
            int traineeId, StartNutritionEnrollmentDto dto)
        {
            // 1. Load plan — must be published.
            var plan = await _uow.Repository<NutritionPlan>().GetAsync(dto.NutritionPlanID);
            if (plan is null || !plan.IsPublished)
                throw new InvalidOperationException("Nutrition plan not found or not published.");

            // 2. Check for an existing active enrollment in this plan.
            var activeSpec = new ActiveNutritionEnrollmentByPlanSpec(traineeId, dto.NutritionPlanID);
            var active     = await _uow.Repository<TraineeNutritionEnrollment>()
                                       .GetWithSpecAsync(activeSpec);
            if (active is not null)
                throw new InvalidOperationException(
                    "You are already actively enrolled in this nutrition plan.");

            // 3. Check for a previous cancelled enrollment to resume from.
            var prevSpec = new PreviousNutritionEnrollmentByPlanSpec(traineeId, dto.NutritionPlanID);
            var previous = await _uow.Repository<TraineeNutritionEnrollment>()
                                     .GetWithSpecAsync(prevSpec);

            // 4. Calculate TDEE to set baseline calories.
            var trainee = await _uow.Repository<Trainee>().GetAsync(traineeId);

            int baselineKcal = await ComputeBaselineAsync(trainee, plan);

            TraineeNutritionEnrollment enrollment;

            if (previous is not null)
            {
                // ── Resume: reactivate the row with the most saved progress ──
                previous.IsActive    = true;
                previous.Status      = NutritionEnrollmentStatus.Active;
                previous.StartDate   = BackdateStartDate(previous.MaxWeekUnlocked);
                previous.EndDate     = null;
                // Preserve CurrentAdjustedKcal from where the trainee left off.
                // Baseline is recalculated in case their weight has changed.
                previous.BaselineCalories = baselineKcal;

                if (dto.LinkedWorkoutEnrollmentID.HasValue)
                    previous.LinkedWorkoutEnrollmentID = dto.LinkedWorkoutEnrollmentID;

                _uow.Repository<TraineeNutritionEnrollment>().Update(previous);
                enrollment = previous;
            }
            else
            {
                // ── Fresh enrollment ──
                enrollment = new TraineeNutritionEnrollment
                {
                    TraineeID                  = traineeId,
                    NutritionPlanID            = dto.NutritionPlanID,
                    LinkedWorkoutEnrollmentID  = dto.LinkedWorkoutEnrollmentID,
                    StartDate                  = DateTime.UtcNow,
                    Status                     = NutritionEnrollmentStatus.Active,
                    IsActive                   = true,
                    MaxWeekUnlocked            = 1,
                    BaselineCalories           = baselineKcal,
                    CurrentAdjustedKcal        = baselineKcal
                };
                _uow.Repository<TraineeNutritionEnrollment>().Add(enrollment);
            }

            await _uow.CompleteAsync();

            // 5. Create default constraints for this enrollment if none exist.
            if (previous is null)
            {
                var defaults = _constraints.BuildDefaults(plan.TrainingGoal, trainee?.Weight ?? 80m);
                // Use the system-internal method — no coach ownership check needed here.
                await _constraints.UpsertConstraintsInternalAsync(enrollment.Id, defaults);
            }

            // 6. Reload with nav props for mapping.
            var loadSpec = new NutritionEnrollmentByIdSpec(enrollment.Id, traineeId);
            var loaded   = await _uow.Repository<TraineeNutritionEnrollment>()
                                     .GetWithSpecAsync(loadSpec);

            return await MapToEnrollmentDtoAsync(loaded!);
        }

        // ── Cancel ───────────────────────────────────────────────────────────

        public async Task CancelEnrollmentAsync(int enrollmentId, int traineeId)
        {
            var spec       = new NutritionEnrollmentByIdSpec(enrollmentId, traineeId);
            var enrollment = await _uow.Repository<TraineeNutritionEnrollment>()
                                       .GetWithSpecAsync(spec);
            if (enrollment is null)
                throw new InvalidOperationException("Enrollment not found.");

            // Soft cancel — MaxWeekUnlocked and all check-in history are preserved.
            enrollment.IsActive = false;
            enrollment.Status   = NutritionEnrollmentStatus.Cancelled;
            enrollment.EndDate  = DateTime.UtcNow;

            _uow.Repository<TraineeNutritionEnrollment>().Update(enrollment);
            await _uow.CompleteAsync();
        }

        // ── TDEE preview ─────────────────────────────────────────────────────

        public async Task<TDEEResultDto> PreviewTDEEAsync(int traineeId, int planId)
        {
            var plan    = await _uow.Repository<NutritionPlan>().GetAsync(planId);
            var trainee = await _uow.Repository<Trainee>().GetAsync(traineeId);

            if (plan    is null) throw new InvalidOperationException("Plan not found.");
            if (trainee is null) throw new InvalidOperationException("Trainee profile not found.");

            int sessionsPerWeek = await GetSessionsPerWeekAsync(plan);

            return _tdee.ComputeFullResult(
                gender:              trainee.Gender,
                weightKg:            trainee.Weight ?? 80m,
                heightCm:            trainee.Height ?? 170m,
                dateOfBirth:         trainee.DateOfBirth,
                sessionsPerWeek:     sessionsPerWeek,
                goal:                plan.TrainingGoal,
                proteinTargetPerKg:  plan.ProteinTargetPerKg,
                fatFloorG:           50,
                strategyType:        plan.CalorieStrategyType,
                absoluteTarget:      plan.AbsoluteCalorieTarget,
                tdeeAdjustment:      plan.TDEEAdjustmentKcal);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CORE: Week unlock sync
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Computes and persists the correct MaxWeekUnlocked for this enrollment.
        ///
        /// Dual-gate algorithm:
        ///   Gate 1 (Time)  — 7-day Monday-anchored calendar weeks from StartDate.
        ///   Gate 2 (Coach) — WeeklyCheckIn.CoachApprovedAt must be set for each
        ///                    week before the next week unlocks.
        ///
        /// Week 1 is always unlocked (no prior check-in required).
        /// Both gates must pass to advance MaxWeekUnlocked.
        /// </summary>
        internal async Task SyncMaxWeekUnlockedAsync(TraineeNutritionEnrollment enrollment)
        {
            var plan = enrollment.NutritionPlan
                       ?? await _uow.Repository<NutritionPlan>().GetAsync(enrollment.NutritionPlanID);
            if (plan is null) return;

            int totalWeeks   = plan.DurationOnWeeks;
            int maxByTime    = ComputeWeeksDue(enrollment.StartDate, totalWeeks);
            int maxByApproval = 1;  // week 1 never requires prior approval

            // Load all check-ins for this enrollment in ascending week order.
            var ciSpec    = new AllCheckInsForEnrollmentSpec(enrollment.Id);
            var checkIns  = (await _uow.Repository<WeeklyCheckIn>().GetAllWithSpecAsync(ciSpec))
                            .ToDictionary(c => c.WeekNumber);

            // Advance maxByApproval for each week that has been coach-approved.
            for (int w = 1; w < maxByTime; w++)
            {
                if (checkIns.TryGetValue(w, out var ci) && ci.CoachApprovedAt.HasValue)
                    maxByApproval = w + 1;
                else
                    break;  // cannot jump past an unapproved week
            }

            int newMax = Math.Min(Math.Min(maxByTime, maxByApproval), totalWeeks);

            if (newMax != enrollment.MaxWeekUnlocked)
            {
                enrollment.MaxWeekUnlocked = newMax;

                // Auto-complete when the trainee reaches the final week.
                if (newMax >= totalWeeks && enrollment.Status == NutritionEnrollmentStatus.Active)
                {
                    enrollment.Status   = NutritionEnrollmentStatus.Completed;
                    enrollment.IsActive = false;
                    enrollment.EndDate  = DateTime.UtcNow;
                }

                _uow.Repository<TraineeNutritionEnrollment>().Update(enrollment);
                await _uow.CompleteAsync();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Monday-anchored week computation — mirrors the training system exactly.
        /// Returns the number of full calendar weeks that have elapsed since StartDate.
        /// Result is clamped to [1, totalWeeks] so it is always a valid week number.
        /// </summary>
        private static int ComputeWeeksDue(DateTime startDate, int totalWeeks)
        {
            var startMonday = GetWeekMonday(startDate.Date);
            var nowMonday   = GetWeekMonday(DateTime.UtcNow.Date);
            int weeksPassed = (int)(nowMonday - startMonday).TotalDays / 7;
            return Math.Min(weeksPassed + 1, totalWeeks);
        }

        /// <summary>
        /// Returns the Monday of the ISO week containing the given date.
        /// DayOfWeek.Sunday = 0, Monday = 1 ... Saturday = 6 in .NET.
        /// The expression (((int)date.DayOfWeek + 6) % 7) converts to Mon=0 … Sun=6.
        /// </summary>
        private static DateTime GetWeekMonday(DateTime date)
            => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

        /// <summary>
        /// Back-dates StartDate so that the time gate reflects the trainee's saved
        /// MaxWeekUnlocked on resume.
        ///
        /// Formula: StartDate = currentMonday − (savedWeeks − 1) × 7 days.
        /// This ensures the time gate immediately allows access up to savedWeeks
        /// without requiring the trainee to wait 7 × savedWeeks real days again.
        /// </summary>
        private static DateTime BackdateStartDate(int savedMaxWeek)
        {
            var currentMonday = GetWeekMonday(DateTime.UtcNow.Date);
            return currentMonday.AddDays(-((savedMaxWeek - 1) * 7));
        }

        private async Task<int> ComputeBaselineAsync(Trainee? trainee, NutritionPlan plan)
        {
            if (trainee is null) return plan.AbsoluteCalorieTarget ?? 2000;

            int sessionsPerWeek = await GetSessionsPerWeekAsync(plan);

            var result = _tdee.ComputeFullResult(
                gender:             trainee.Gender ?? "Male",
                weightKg:           trainee.Weight ?? 80m,
                heightCm:           trainee.Height ?? 170m,
                dateOfBirth:        trainee.DateOfBirth,
                sessionsPerWeek:    sessionsPerWeek,
                goal:               plan.TrainingGoal,
                proteinTargetPerKg: plan.ProteinTargetPerKg,
                fatFloorG:          50,
                strategyType:       plan.CalorieStrategyType,
                absoluteTarget:     plan.AbsoluteCalorieTarget,
                tdeeAdjustment:     plan.TDEEAdjustmentKcal);

            return result.AdjustedCalories;
        }

        /// <summary>
        /// Reads sessions per week from the linked WorkoutProgram if available,
        /// otherwise falls back to the plan's declared SessionsPerWeeks-equivalent.
        /// For nutrition plans not linked to a program, defaults to 4 (moderate).
        /// </summary>
        private async Task<int> GetSessionsPerWeekAsync(NutritionPlan plan)
        {
            if (plan.LinkedWorkoutProgramID.HasValue)
            {
                var prog = await _uow.Repository<WorkoutProgram>()
                                     .GetAsync(plan.LinkedWorkoutProgramID.Value);
                if (prog is not null) return prog.SessionsPerWeeks;
            }
            return 4; // sensible default for moderate activity
        }

        private async Task<Trainee?> GetTraineeByIdAsync(int traineeId)
            => await _uow.Repository<Trainee>().GetAsync(traineeId);

        private async Task<NutritionEnrollmentDto> MapToEnrollmentDtoAsync(
            TraineeNutritionEnrollment e)
        {
            // Determine check-in status flags.
            var latestCiSpec = new LatestCheckInForEnrollmentSpec(e.Id);
            var latestCi     = await _uow.Repository<WeeklyCheckIn>()
                                         .GetWithSpecAsync(latestCiSpec);

            bool pendingCheckIn     = latestCi is null || latestCi.WeekNumber < e.MaxWeekUnlocked;
            bool pendingCoachReview = latestCi is not null && latestCi.CoachApprovedAt is null;

            return new NutritionEnrollmentDto
            {
                Id                  = e.Id,
                NutritionPlanID     = e.NutritionPlanID,
                PlanName            = e.NutritionPlan?.Name ?? string.Empty,
                CoachName           = e.NutritionPlan?.Coach?.ApplicationUser?.FullName ?? string.Empty,
                MaxWeekUnlocked     = e.MaxWeekUnlocked,
                TotalWeeks          = e.NutritionPlan?.DurationOnWeeks ?? 0,
                Status              = e.Status.ToString(),
                StartDate           = e.StartDate,
                EndDate             = e.EndDate,
                BaselineCalories    = e.BaselineCalories,
                CurrentAdjustedKcal = e.CurrentAdjustedKcal,
                PendingCheckIn      = pendingCheckIn,
                PendingCoachReview  = pendingCoachReview
            };
        }
    }
}
