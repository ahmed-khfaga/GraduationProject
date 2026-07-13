using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.NutritionDTOs;
using FitZone.Service.Services.Contract;
using FitZone.Core.Specifications.Params;
namespace FitZone.Service
{
    public class NutritionPlanService : INutritionPlanService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper     _mapper;

        public NutritionPlanService(IUnitOfWork uow, IMapper mapper)
        {
            _uow    = uow;
            _mapper = mapper;
        }

        // ── Public catalogue ─────────────────────────────────────────────────

        public async Task<PaginatedResult<NutritionPlanCardDto>> GetPublishedPlansAsync(
            NutritionPlanFilterParams filters)
        {
            var dataSpec  = new PublishedNutritionPlansSpec(filters);
            var countSpec = new PublishedNutritionPlansSpec(filters, countOnly: true);

            var plans = await _uow.Repository<NutritionPlan>().GetAllWithSpecAsync(dataSpec);
            var total = await _uow.Repository<NutritionPlan>().CountAsync(countSpec);

            return new PaginatedResult<NutritionPlanCardDto>
            {
                PageIndex  = filters.PageIndex,
                PageSize   = filters.PageSize,
                TotalCount = total,
                Data       = _mapper.Map<IEnumerable<NutritionPlanCardDto>>(plans)
            };
        }

        public async Task<NutritionPlanDetailDto?> GetPlanDetailAsync(int planId)
        {
            var spec = new NutritionPlanWithWeeksSpec(planId);
            var plan = await _uow.Repository<NutritionPlan>().GetWithSpecAsync(spec);
            return plan is null ? null : _mapper.Map<NutritionPlanDetailDto>(plan);
        }

        public async Task<IEnumerable<NutritionPlanCardDto>> GetCoachPlansAsync(int coachId)
        {
            var spec  = new CoachNutritionPlansSpec(coachId);
            var plans = await _uow.Repository<NutritionPlan>().GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<NutritionPlanCardDto>>(plans);
        }

        public async Task<NutritionWeekCoachDetailDto?> GetWeekDetailForCoachAsync(int weekId, int coachId)
        {
            var spec = new NutritionWeekByCoachWithDetailSpec(weekId, coachId);
            var week = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(spec);
            return week is null ? null : _mapper.Map<NutritionWeekCoachDetailDto>(week);
        }

        public async Task<NutritionWeekCoachDetailDto?> AdminGetWeekDetailAsync(int weekId)
        {
            var spec = new NutritionWeekByIdWithDetailSpec(weekId);
            var week = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(spec);
            return week is null ? null : _mapper.Map<NutritionWeekCoachDetailDto>(week);
        }
        // ── Plan CRUD ────────────────────────────────────────────────────────

        public async Task<int> CreatePlanAsync(int coachId, CreateNutritionPlanDto dto)
        {
            var plan = _mapper.Map<NutritionPlan>(dto);
            plan.CoachID = coachId;   // always injected from JWT — never from body

            _uow.Repository<NutritionPlan>().Add(plan);
            await _uow.CompleteAsync();
            return plan.Id;
        }

        public async Task<bool> UpdatePlanAsync(int planId, int coachId, UpdateNutritionPlanDto dto)
        {
            var spec = new NutritionPlanByCoachSpec(planId, coachId);
            var plan = await _uow.Repository<NutritionPlan>().GetWithSpecAsync(spec);
            if (plan is null) return false;

            // TrackID is not a concept in nutrition — but LinkedWorkoutProgramID CAN change
            // because it is an optional loose coupling, not a hard track assignment.
            _mapper.Map(dto, plan);
            plan.CoachID = coachId;   // guard: mapping must not overwrite the injected coach
            _uow.Repository<NutritionPlan>().Update(plan);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeletePlanAsync(int planId, int coachId)
        {
            var spec = new NutritionPlanByCoachSpec(planId, coachId);
            var plan = await _uow.Repository<NutritionPlan>().GetWithSpecAsync(spec);
            if (plan is null) return false;

            _uow.Repository<NutritionPlan>().Delete(plan);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> AdminDeletePlanAsync(int planId)
        {
            var plan = await _uow.Repository<NutritionPlan>().GetAsync(planId);
            if (plan is null) return false;

            // TraineeNutritionEnrollment → NutritionPlan uses DeleteBehavior.NoAction
            // at the database level (deliberately — enrollment history must never be
            // silently destroyed by deleting the plan it points to). Check for this
            // up front and fail with a clear message instead of letting the raw
            // SqlException surface as an opaque 500 "error occurred while saving."
            var enrollmentSpec = new EnrollmentsByPlanIdSpec(planId);
            var hasEnrollments = await _uow.Repository<TraineeNutritionEnrollment>()
                .GetAllWithSpecAsync(enrollmentSpec);

            if (hasEnrollments.Any())
                throw new InvalidOperationException(
                    $"Cannot delete plan {planId}: {hasEnrollments.Count()} trainee enrollment(s) " +
                    "still reference it (active or historical). Enrollment records are preserved " +
                    "deliberately and are never cascade-deleted with the plan. If this plan should " +
                    "never have had trainees on it, contact engineering to review those enrollments " +
                    "before forcing a delete.");

            _uow.Repository<NutritionPlan>().Delete(plan);
            await _uow.CompleteAsync();
            return true;
        }

        // ── Publish / unpublish ──────────────────────────────────────────────

        public async Task<bool> PublishPlanAsync(int planId, int coachId)
        {
            var spec = new NutritionPlanByCoachSpec(planId, coachId);
            var plan = await _uow.Repository<NutritionPlan>().GetWithSpecAsync(spec);
            if (plan is null) return false;

            plan.IsPublished = true;
            plan.PublishedAt = DateTime.UtcNow;
            _uow.Repository<NutritionPlan>().Update(plan);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> UnpublishPlanAsync(int planId, int coachId)
        {
            var spec = new NutritionPlanByCoachSpec(planId, coachId);
            var plan = await _uow.Repository<NutritionPlan>().GetWithSpecAsync(spec);
            if (plan is null) return false;

            // Active enrollees are NEVER affected — only the public catalogue entry changes.
            plan.IsPublished = false;
            _uow.Repository<NutritionPlan>().Update(plan);
            await _uow.CompleteAsync();
            return true;
        }

        // ── Week management ──────────────────────────────────────────────────

        /// <summary>
        /// Adds a complete week — NutritionWeek, all DayProtocols, all Meals, and all
        /// MealFoodItems — in a single UoW transaction.
        /// ExerciseID ownership is validated before insertion (global OR coach-private).
        /// </summary>
        public async Task AddNutritionWeekAsync(
            int planId, int coachId, CreateNutritionWeekDto dto)
        {
            // 1. Verify coach owns this plan.
            var planSpec = new NutritionPlanByCoachSpec(planId, coachId);
            var plan     = await _uow.Repository<NutritionPlan>().GetWithSpecAsync(planSpec);
            if (plan is null)
                throw new InvalidOperationException("Nutrition plan not found or access denied.");

            // 2. Prevent duplicate week numbers.
            var existingSpec = new NutritionWeekFullDetailSpec(planId, dto.WeekNumber);
            var existing     = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(existingSpec);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"Week {dto.WeekNumber} already exists in this plan.");

            // 3. Build the full object graph.
            var week = new NutritionWeek
            {
                NutritionPlanID  = planId,
                WeekNumber       = dto.WeekNumber,
                WeekProtocolType = dto.WeekProtocolType,
                CalorieModifier  = dto.CalorieModifier,
                WeekDescription  = dto.WeekDescription,
                FocusNote        = dto.FocusNote,
                ProgressionNote  = dto.ProgressionNote,
                NextWeekPreview  = dto.NextWeekPreview
            };

            foreach (var dtoProtocol in dto.DayProtocols)
            {
                var protocol = new DayProtocol
                {
                    DayProtocolType       = dtoProtocol.DayProtocolType,
                    WeekDay               = dtoProtocol.WeekDay,
                    DayOrder              = dtoProtocol.DayOrder,
                    LinkedWorkoutSessionID = dtoProtocol.LinkedWorkoutSessionID,
                    TotalCaloriesTarget   = dtoProtocol.TotalCaloriesTarget,
                    ProteinTargetG        = dtoProtocol.ProteinTargetG,
                    CarbTargetG           = dtoProtocol.CarbTargetG,
                    FatTargetG            = dtoProtocol.FatTargetG,
                    ProtocolNotes         = dtoProtocol.ProtocolNotes
                };

                foreach (var dtoMeal in dtoProtocol.Meals)
                {
                    var meal = new Meal
                    {
                        Name                    = dtoMeal.Name,
                        TimingType              = dtoMeal.TimingType,
                        MealOrder               = dtoMeal.MealOrder,
                        TimeFromTrainingMinutes = dtoMeal.TimeFromTrainingMinutes,
                        TargetCalories          = dtoMeal.TargetCalories,
                        TargetProteinG          = dtoMeal.TargetProteinG,
                        TargetCarbG             = dtoMeal.TargetCarbG,
                        TargetFatG              = dtoMeal.TargetFatG,
                        Notes                   = dtoMeal.Notes
                    };

                    foreach (var dtoFoodItem in dtoMeal.FoodItems)
                    {
                        // Validate: food item must be global (CoachID == null) or
                        // owned by this coach — same rule as exercise assignment.
                        var foodSpec = new FoodItemByIdForCoachSpec(dtoFoodItem.FoodItemID, coachId);
                        var food     = await _uow.Repository<FoodItem>().GetWithSpecAsync(foodSpec);
                        if (food is null)
                            throw new InvalidOperationException(
                                $"Food item ID {dtoFoodItem.FoodItemID} is not available to this coach.");

                        meal.MealFoodItems.Add(new MealFoodItem
                        {
                            FoodItemID  = dtoFoodItem.FoodItemID,
                            AmountGrams = dtoFoodItem.AmountGrams,
                            IsOptional  = dtoFoodItem.IsOptional,
                            SwapGroupID = dtoFoodItem.SwapGroupID
                        });
                    }

                    protocol.Meals.Add(meal);
                }

                week.DayProtocols.Add(protocol);
            }

            _uow.Repository<NutritionWeek>().Add(week);
            await _uow.CompleteAsync();
        }

        public async Task<bool> UpdateNutritionWeekAsync(
            int weekId, int coachId, UpdateNutritionWeekDto dto)
        {
            var spec = new NutritionWeekByCoachSpec(weekId, coachId);
            var week = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(spec);
            if (week is null) return false;

            // Patch only the provided non-null fields.
            if (dto.WeekProtocolType.HasValue) week.WeekProtocolType = dto.WeekProtocolType.Value;
            if (dto.CalorieModifier.HasValue)  week.CalorieModifier  = dto.CalorieModifier.Value;
            if (dto.WeekDescription  is not null) week.WeekDescription  = dto.WeekDescription;
            if (dto.FocusNote        is not null) week.FocusNote        = dto.FocusNote;
            if (dto.ProgressionNote  is not null) week.ProgressionNote  = dto.ProgressionNote;
            if (dto.NextWeekPreview  is not null) week.NextWeekPreview  = dto.NextWeekPreview;

            _uow.Repository<NutritionWeek>().Update(week);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteNutritionWeekAsync(int weekId, int coachId)
        {
            var spec = new NutritionWeekByCoachSpec(weekId, coachId);
            var week = await _uow.Repository<NutritionWeek>().GetWithSpecAsync(spec);
            if (week is null) return false;

            _uow.Repository<NutritionWeek>().Delete(week);
            await _uow.CompleteAsync();
            return true;
        }

        // ── Day protocol management ──────────────────────────────────────────

        public async Task<bool> UpdateDayProtocolAsync(
            int dayProtocolId, int coachId, UpdateDayProtocolDto dto)
        {
            var spec     = new DayProtocolByCoachSpec(dayProtocolId, coachId);
            var protocol = await _uow.Repository<DayProtocol>().GetWithSpecAsync(spec);
            if (protocol is null) return false;

            if (dto.TotalCaloriesTarget.HasValue) protocol.TotalCaloriesTarget = dto.TotalCaloriesTarget.Value;
            if (dto.ProteinTargetG.HasValue)      protocol.ProteinTargetG      = dto.ProteinTargetG.Value;
            if (dto.CarbTargetG.HasValue)         protocol.CarbTargetG         = dto.CarbTargetG.Value;
            if (dto.FatTargetG.HasValue)          protocol.FatTargetG          = dto.FatTargetG.Value;
            if (dto.ProtocolNotes is not null)    protocol.ProtocolNotes       = dto.ProtocolNotes;

            _uow.Repository<DayProtocol>().Update(protocol);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteDayProtocolAsync(int dayProtocolId, int coachId)
        {
            var spec     = new DayProtocolByCoachSpec(dayProtocolId, coachId);
            var protocol = await _uow.Repository<DayProtocol>().GetWithSpecAsync(spec);
            if (protocol is null) return false;

            _uow.Repository<DayProtocol>().Delete(protocol);
            await _uow.CompleteAsync();
            return true;
        }

        // ── Meal management ──────────────────────────────────────────────────

        public async Task<bool> UpdateMealAsync(int mealId, int coachId, UpdateMealDto dto)
        {
            var meal = await _uow.Repository<Meal>().GetAsync(mealId);
            if (meal is null) return false;

            // Verify ownership by loading protocol → week → plan.
            var protocolSpec = new DayProtocolByCoachSpec(meal.DayProtocolID, coachId);
            var protocol     = await _uow.Repository<DayProtocol>().GetWithSpecAsync(protocolSpec);
            if (protocol is null) return false;

            if (dto.Name                    is not null) meal.Name                    = dto.Name;
            if (dto.TimingType.HasValue)                 meal.TimingType              = dto.TimingType.Value;
            if (dto.MealOrder.HasValue)                  meal.MealOrder               = dto.MealOrder.Value;
            if (dto.TimeFromTrainingMinutes.HasValue)    meal.TimeFromTrainingMinutes = dto.TimeFromTrainingMinutes;
            if (dto.TargetCalories.HasValue)             meal.TargetCalories          = dto.TargetCalories.Value;
            if (dto.TargetProteinG.HasValue)             meal.TargetProteinG          = dto.TargetProteinG.Value;
            if (dto.TargetCarbG.HasValue)                meal.TargetCarbG             = dto.TargetCarbG.Value;
            if (dto.TargetFatG.HasValue)                 meal.TargetFatG              = dto.TargetFatG.Value;
            if (dto.Notes is not null)                   meal.Notes                   = dto.Notes;

            _uow.Repository<Meal>().Update(meal);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteMealAsync(int mealId, int coachId)
        {
            var meal = await _uow.Repository<Meal>().GetAsync(mealId);
            if (meal is null) return false;

            var protocolSpec = new DayProtocolByCoachSpec(meal.DayProtocolID, coachId);
            var protocol     = await _uow.Repository<DayProtocol>().GetWithSpecAsync(protocolSpec);
            if (protocol is null) return false;

            _uow.Repository<Meal>().Delete(meal);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<int> AddMealFoodItemAsync(
    int mealId, int coachId, AddMealFoodItemDto dto)
        {
            // Verify coach owns the meal's plan.
            var mealSpec = new MealWithFoodItemsSpec(mealId);
            var meal = await _uow.Repository<Meal>().GetWithSpecAsync(mealSpec);
            if (meal is null) throw new InvalidOperationException("Meal not found.");

            var protSpec = new DayProtocolByCoachSpec(meal.DayProtocolID, coachId);
            var protocol = await _uow.Repository<DayProtocol>().GetWithSpecAsync(protSpec);
            if (protocol is null)
                throw new InvalidOperationException("Access denied or day protocol not found.");

            var assignment = new MealFoodItem
            {
                MealID = mealId,
                FoodItemID = dto.FoodItemID,
                AmountGrams = dto.AmountGrams,
                IsOptional = dto.IsOptional,
                SwapGroupID = dto.SwapGroupID
            };

            _uow.Repository<MealFoodItem>().Add(assignment);
            await _uow.CompleteAsync();
            return assignment.Id;
        }

        public async Task<bool> UpdateMealFoodItemAsync(
            int mealFoodItemId, int coachId, UpdateMealFoodItemDto dto)
        {
            var mfi = await _uow.Repository<MealFoodItem>().GetAsync(mealFoodItemId);
            if (mfi is null) return false;

            var meal = await _uow.Repository<Meal>().GetAsync(mfi.MealID);
            if (meal is null) return false;

            var protSpec = new DayProtocolByCoachSpec(meal.DayProtocolID, coachId);
            var protocol = await _uow.Repository<DayProtocol>().GetWithSpecAsync(protSpec);
            if (protocol is null) return false;

            // Partial update — only apply non-null fields.
            if (dto.AmountGrams.HasValue) mfi.AmountGrams = dto.AmountGrams.Value;
            if (dto.IsOptional.HasValue) mfi.IsOptional = dto.IsOptional.Value;
            if (dto.SwapGroupID.HasValue) mfi.SwapGroupID = dto.SwapGroupID;

            _uow.Repository<MealFoodItem>().Update(mfi);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteMealFoodItemAsync(int mealFoodItemId, int coachId)
        {
            var mfi = await _uow.Repository<MealFoodItem>().GetAsync(mealFoodItemId);
            if (mfi is null) return false;

            var meal = await _uow.Repository<Meal>().GetAsync(mfi.MealID);
            if (meal is null) return false;

            var protSpec = new DayProtocolByCoachSpec(meal.DayProtocolID, coachId);
            var protocol = await _uow.Repository<DayProtocol>().GetWithSpecAsync(protSpec);
            if (protocol is null) return false;

            _uow.Repository<MealFoodItem>().Delete(mfi);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
