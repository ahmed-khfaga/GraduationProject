using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Services.Contract;
using FitZone.Core.Specifications.Params;
// ══════════════════════════════════════════════════════════════════════════════
// FOOD ITEM SERVICE
// ══════════════════════════════════════════════════════════════════════════════
namespace FitZone.Service
{
    /// <summary>
    /// Manages the food library — global items (CoachID = null, read-only) and
    /// coach-private items (CoachID set, full CRUD). Mirrors ExerciseService exactly.
    /// </summary>
    public class FoodItemService : IFoodItemService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper     _mapper;

        public FoodItemService(IUnitOfWork uow, IMapper mapper)
        {
            _uow    = uow;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<FoodItemSummaryDto>> GetFoodItemsForCoachAsync(
            int coachId, FoodItemFilterParams filters)
        {
            var dataSpec  = new FoodItemsForCoachSpec(coachId, filters);
            var countSpec = new FoodItemsForCoachSpec(coachId, filters, countOnly: true);

            var items = await _uow.Repository<FoodItem>().GetAllWithSpecAsync(dataSpec);
            var total = await _uow.Repository<FoodItem>().CountAsync(countSpec);

            return new PaginatedResult<FoodItemSummaryDto>
            {
                PageIndex  = filters.PageIndex,
                PageSize   = filters.PageSize,
                TotalCount = total,
                Data       = _mapper.Map<IEnumerable<FoodItemSummaryDto>>(items)
            };
        }

        public async Task<FoodItemDetailDto?> GetFoodItemByIdForCoachAsync(int id, int coachId)
        {
            var spec = new FoodItemByIdForCoachSpec(id, coachId);
            var item = await _uow.Repository<FoodItem>().GetWithSpecAsync(spec);
            return item is null ? null : _mapper.Map<FoodItemDetailDto>(item);
        }

        public async Task<int> CreateFoodItemAsync(DTOs.NutritionDTOs.CreateFoodItemDto dto, int coachId)
        {
            var item = _mapper.Map<FoodItem>(dto);
            item.CoachID = coachId;   // always injected from JWT — never from body

            _uow.Repository<FoodItem>().Add(item);
            await _uow.CompleteAsync();
            return item.Id;
        }

        public async Task<bool> UpdateFoodItemAsync(int id, DTOs.NutritionDTOs.CreateFoodItemDto dto, int coachId)
        {
            var spec = new FoodItemByIdForCoachSpec(id, coachId);
            var item = await _uow.Repository<FoodItem>().GetWithSpecAsync(spec);
            if (item is null) return false;

            // Global food items (CoachID == null) are read-only — block any write.
            if (item.CoachID is null)
                throw new InvalidOperationException(
                    "Global food items cannot be edited. " +
                    "Create a private copy if you need a custom version.");

            _mapper.Map(dto, item);
            item.CoachID = coachId;   // guard against mapping overwriting the owner

            _uow.Repository<FoodItem>().Update(item);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteFoodItemAsync(int id, int coachId)
        {
            var spec = new FoodItemByIdForCoachSpec(id, coachId);
            var item = await _uow.Repository<FoodItem>().GetWithSpecAsync(spec);
            if (item is null) return false;

            // Global food items are protected from deletion.
            if (item.CoachID is null)
                throw new InvalidOperationException(
                    "Global food items cannot be deleted. " +
                    "Only private food items you created can be removed.");

            _uow.Repository<FoodItem>().Delete(item);
            await _uow.CompleteAsync();
            return true;
        }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// CONSTRAINT SERVICE
// ══════════════════════════════════════════════════════════════════════════════
namespace FitZone.Service
{
    /// <summary>
    /// Manages per-client constraint settings. These are the guardrails the coach
    /// sets for the ProposalEngine — not global plan settings but individual
    /// configurations tailored to each trainee's characteristics.
    ///
    /// BuildDefaults provides sensible starting points based on training goal,
    /// which the coach can then refine as they observe the trainee's responses.
    /// </summary>
    public class ConstraintService : IConstraintService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper     _mapper;

        public ConstraintService(IUnitOfWork uow, IMapper mapper)
        {
            _uow    = uow;
            _mapper = mapper;
        }

        public async Task<ConstraintsDto?> GetConstraintsAsync(int enrollmentId, int coachId)
        {
            // Verify coach owns the plan of this enrollment.
            var enrollSpec  = new NutritionEnrollmentByIdForCoachSpec(enrollmentId, coachId);
            var enrollment  = await _uow.Repository<TraineeNutritionEnrollment>()
                                        .GetWithSpecAsync(enrollSpec);
            if (enrollment is null) return null;

            var spec        = new ConstraintsByEnrollmentSpec(enrollmentId);
            var constraints = await _uow.Repository<ClientNutritionConstraints>()
                                        .GetWithSpecAsync(spec);
            return constraints is null ? null : _mapper.Map<ConstraintsDto>(constraints);
        }

        /// <summary>
        /// Coach-facing upsert. Always verifies coach ownership before writing.
        /// </summary>
        public async Task UpsertConstraintsAsync(int enrollmentId, int coachId, DTOs.NutritionDTOs.SetConstraintsDto dto)
        {
            var enrollSpec = new NutritionEnrollmentByIdForCoachSpec(enrollmentId, coachId);
            var enrollment = await _uow.Repository<TraineeNutritionEnrollment>()
                                       .GetWithSpecAsync(enrollSpec)
                          ?? throw new InvalidOperationException(
                                 "Enrollment not found or you do not own the plan.");

            await PersistConstraintsAsync(enrollmentId, dto);
        }

        /// <summary>
        /// System-internal upsert. Called only by NutritionEnrollmentService during enrollment setup.
        /// Never called from a controller.
        /// </summary>
        internal async Task UpsertConstraintsInternalAsync(int enrollmentId, SetConstraintsDto dto)
            => await PersistConstraintsAsync(enrollmentId, dto);

        // Shared persistence — no ownership check. Only called by the two methods above.
        private async Task PersistConstraintsAsync(int enrollmentId, SetConstraintsDto dto)
        {
            var spec = new ConstraintsByEnrollmentSpec(enrollmentId);
            var existing = await _uow.Repository<ClientNutritionConstraints>()
                                     .GetWithSpecAsync(spec);
            if (existing is null)
            {
                var newConstraints = _mapper.Map<ClientNutritionConstraints>(dto);
                newConstraints.EnrollmentID = enrollmentId;
                _uow.Repository<ClientNutritionConstraints>().Add(newConstraints);
            }
            else
            {
                _mapper.Map(dto, existing);
                _uow.Repository<ClientNutritionConstraints>().Update(existing);
            }
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Builds sensible default constraints from the plan's TrainingGoal and the
        /// trainee's body weight. The coach will refine these after seeing 2–3 weeks
        /// of real response data.
        /// </summary>
        public SetConstraintsDto BuildDefaults(TrainingGoal goal, decimal weightKg)
        {
            var defaults = new SetConstraintsDto
            {
                WeightAveragingDays              = 3,
                DeviationTriggerKg               = 0.15m,
                ProteinFloorG                    = (int)Math.Round(weightKg * 1.8m),
                FatFloorG                        = 50,
                MaxSingleAdjustmentKcal          = 150,
                MaxCumulativeDriftKcal           = 500,
                PreferredAdjustmentVector        = AdjustmentVector.RestDayCarbs,
                AdherenceThresholdPercent        = 75,
                RequireConsecutiveWeeksDeviation = false,
                ApplyTrainingWeekNoiseCorrection = true,
                EnergyLevelEscalationRule        = true,
                PreserveLeanMassOverRate         = false,
                EnableBaselineRecalibrationReview = true
            };

            // Goal-specific expected weekly change ranges and calorie bounds.
            switch (goal)
            {
                case TrainingGoal.LoseFat:
                    defaults.ExpectedWeeklyChangeMin = -0.60m;
                    defaults.ExpectedWeeklyChangeMax = -0.35m;
                    defaults.CalorieFloor            = 1600;
                    defaults.CalorieCeiling          = 3500;
                    defaults.PreserveLeanMassOverRate = true;   // protect muscle during cut
                    break;

                case TrainingGoal.BuildMuscle:
                    defaults.ExpectedWeeklyChangeMin = +0.10m;
                    defaults.ExpectedWeeklyChangeMax = +0.25m;
                    defaults.CalorieFloor            = 1800;
                    defaults.CalorieCeiling          = 5000;
                    defaults.PreferredAdjustmentVector = AdjustmentVector.TrainingDayCarbs;
                    break;

                case TrainingGoal.GetStronger:
                    defaults.ExpectedWeeklyChangeMin = -0.10m;
                    defaults.ExpectedWeeklyChangeMax = +0.15m;
                    defaults.CalorieFloor            = 1800;
                    defaults.CalorieCeiling          = 5000;
                    defaults.DeviationTriggerKg      = 0.20m;  // more tolerance near maintenance
                    break;

                case TrainingGoal.ImproveEndurance:
                    defaults.ExpectedWeeklyChangeMin = -0.30m;
                    defaults.ExpectedWeeklyChangeMax = +0.05m;
                    defaults.CalorieFloor            = 1800;
                    defaults.CalorieCeiling          = 4500;
                    defaults.PreferredAdjustmentVector = AdjustmentVector.Proportional;
                    break;

                case TrainingGoal.MaintainWeight:
                case TrainingGoal.GeneralFitness:
                case TrainingGoal.MoveBetter:
                default:
                    defaults.ExpectedWeeklyChangeMin = -0.20m;
                    defaults.ExpectedWeeklyChangeMax = +0.15m;
                    defaults.CalorieFloor            = 1600;
                    defaults.CalorieCeiling          = 4500;
                    defaults.DeviationTriggerKg      = 0.20m;
                    break;
            }

            return defaults;
        }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// COACH REVIEW SERVICE
// ══════════════════════════════════════════════════════════════════════════════
namespace FitZone.Service
{
    /// <summary>
    /// Builds the coach's weekly review queue and provides the full review detail
    /// for a single check-in. The queue is sorted by priority so the most
    /// urgent cases appear first:
    ///
    ///   Escalated  — energy escalation fired OR 3+ consecutive weeks off track.
    ///   Proposal   — system has generated a proposal awaiting coach decision.
    ///   OnTrack    — trainee is on track; quick review recommended.
    ///   NoCheckIn  — trainee did not submit; coach must decide whether to unlock.
    /// </summary>
    public class CoachReviewService : ICoachReviewService
    {
        private readonly IUnitOfWork    _uow;
        private readonly ICheckInService _checkInService;

        public CoachReviewService(IUnitOfWork uow, ICheckInService checkInService)
        {
            _uow             = uow;
            _checkInService  = checkInService;
        }

        public async Task<IEnumerable<CoachReviewQueueItemDto>> GetReviewQueueAsync(int coachId)
        {
            // Load all active enrollments across all plans owned by this coach.
            var enrollSpec  = new PendingCoachReviewEnrollmentsSpec(coachId);
            var enrollments = (await _uow.Repository<TraineeNutritionEnrollment>()
                                         .GetAllWithSpecAsync(enrollSpec))
                              .ToList();

            var queue = new List<CoachReviewQueueItemDto>();

            foreach (var enrollment in enrollments)
            {
                // Latest check-in for this enrollment.
                var latestCiSpec = new LatestCheckInForEnrollmentSpec(enrollment.Id);
                var latestCi     = await _uow.Repository<WeeklyCheckIn>()
                                             .GetWithSpecAsync(latestCiSpec);

                // All check-ins for trend analysis.
                var historySpec = new AllCheckInsForEnrollmentSpec(enrollment.Id);
                var history     = (await _uow.Repository<WeeklyCheckIn>()
                                             .GetAllWithSpecAsync(historySpec))
                                  .OrderByDescending(c => c.WeekNumber)
                                  .ToList();

                var item = BuildQueueItem(enrollment, latestCi, history);
                queue.Add(item);
            }

            // Sort: Escalated first, then Proposal, then OnTrack, then NoCheckIn.
            return queue.OrderBy(q => (int)q.Priority).ThenBy(q => q.TraineeName);
        }

        public async Task<CheckInProposalDto?> GetReviewDetailAsync(int checkInId, int coachId)
            => await _checkInService.GetProposalAsync(checkInId, coachId);

        // ── Private: Queue item builder ──────────────────────────────────────

        private static CoachReviewQueueItemDto BuildQueueItem(
            TraineeNutritionEnrollment  enrollment,
            WeeklyCheckIn?              latestCi,
            IList<WeeklyCheckIn>        history)
        {
            var trainee = enrollment.Trainee;
            string traineeName = trainee?.ApplicationUser?.FullName ?? "Unknown";

            // Determine priority.
            ReviewPriority priority;
            string priorityLabel;

            if (latestCi is null)
            {
                // No check-in submitted for current week.
                priority      = ReviewPriority.NoCheckIn;
                priorityLabel = "No check-in submitted";
            }
            else if (latestCi.CoachApprovedAt.HasValue)
            {
                // Already reviewed — determine health of progress.
                priority      = ReviewPriority.OnTrack;
                priorityLabel = "On track — reviewed";
            }
            else if (IsEscalated(latestCi, history, enrollment.Constraints))
            {
                priority      = ReviewPriority.Escalated;
                priorityLabel = "Escalated — requires attention";
            }
            else
            {
                priority      = ReviewPriority.Proposal;
                priorityLabel = "Proposal pending your decision";
            }

            // Weight delta vs previous check-in.
            decimal? delta = null;
            if (latestCi is not null && history.Count >= 2)
            {
                var prev = history
                    .Where(c => c.WeekNumber < latestCi.WeekNumber && c.AverageWeight > 0)
                    .OrderByDescending(c => c.WeekNumber)
                    .FirstOrDefault();
                if (prev is not null)
                    delta = latestCi.AverageWeight - prev.AverageWeight;
            }

            return new CoachReviewQueueItemDto
            {
                EnrollmentId           = enrollment.Id,
                CheckInId              = latestCi?.Id ?? 0,
                TraineeName            = traineeName,
                PlanName               = enrollment.NutritionPlan?.Name ?? string.Empty,
                WeekNumber             = enrollment.MaxWeekUnlocked,
                TotalWeeks             = enrollment.NutritionPlan?.DurationOnWeeks ?? 0,
                Priority               = priority,
                PriorityLabel          = priorityLabel,
                AverageWeight          = latestCi?.AverageWeight,
                WeightDeltaKg          = delta,
                AdherencePercent       = latestCi?.AdherencePercent,
                HasClientNote          = !string.IsNullOrEmpty(latestCi?.ClientNote),
                NoteCategory           = latestCi?.NoteCategory?.ToString(),
                ProposedAdjustmentKcal = latestCi?.SystemProposalKcal,
                SystemConfidence       = latestCi?.SystemConfidence.ToString()
            };
        }

        /// <summary>
        /// Determines if this enrollment meets the escalation condition:
        ///   — Energy level == 1 for 2+ consecutive weeks (energy escalation rule), OR
        ///   — Weight trend is outside expected range for 3+ consecutive weeks.
        /// </summary>
        private static bool IsEscalated(
            WeeklyCheckIn              latestCi,
            IList<WeeklyCheckIn>       history,
            ClientNutritionConstraints? constraints)
        {
            if (constraints is null) return false;

            // Energy escalation: current AND previous week both reported energy = 1.
            if (constraints.EnergyLevelEscalationRule && latestCi.EnergyLevel == 1)
            {
                var prevCi = history
                    .Where(c => c.WeekNumber < latestCi.WeekNumber)
                    .OrderByDescending(c => c.WeekNumber)
                    .FirstOrDefault();
                if (prevCi?.EnergyLevel == 1) return true;
            }

            // Progress escalation: 3+ consecutive weeks outside expected range.
            var recentThree = history
                .Where(c => c.AverageWeight > 0)
                .OrderByDescending(c => c.WeekNumber)
                .Take(3)
                .ToList();

            if (recentThree.Count < 3) return false;

            bool allOutOfRange = true;
            for (int i = 0; i < recentThree.Count - 1; i++)
            {
                decimal delta     = recentThree[i].AverageWeight - recentThree[i + 1].AverageWeight;
                bool    inRange   = delta >= constraints.ExpectedWeeklyChangeMin &&
                                    delta <= constraints.ExpectedWeeklyChangeMax;
                if (inRange) { allOutOfRange = false; break; }
            }

            return allOutOfRange;
        }
    }
}
