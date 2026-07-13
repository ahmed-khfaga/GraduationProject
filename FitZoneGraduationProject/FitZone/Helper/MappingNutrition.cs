
// ══════════════════════════════════════════════════════════════════════════════
// AUTOMAPPER PROFILES  —  one Profile class per entity family
// File: FitZone.APIs/Helper/MappingNutrition.cs
// ══════════════════════════════════════════════════════════════════════════════
using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs.NutritionDTOs;

namespace FitZone.APIs.Helper
{
    // ── NutritionPlan ──────────────────────────────────────────────────────────

    public class MappingNutritionPlan : Profile
    {
        public MappingNutritionPlan()
        {
            // NutritionPlan → NutritionPlanCardDto
            // All enum properties are exposed as strings — clients never need the integer.
            // CoachName and CoachRating traverse the navigation property chain:
            //   NutritionPlan.Coach.ApplicationUser.FullName
            // IsLinkedToProgram is derived (not a stored field).
            CreateMap<NutritionPlan, NutritionPlanCardDto>()
                .ForMember(d => d.CoachName,
                    o => o.MapFrom(s => s.Coach.ApplicationUser.FullName))
                .ForMember(d => d.CoachRating,
                    o => o.MapFrom(s => s.Coach.Rating))
                .ForMember(d => d.TrainingGoal,
                    o => o.MapFrom(s => s.TrainingGoal.ToString()))
                .ForMember(d => d.FitnessLevel,
                    o => o.MapFrom(s => s.FitnessLevel.ToString()))
                .ForMember(d => d.EquipmentType,
                    o => o.MapFrom(s => s.EquipmentType.ToString()))
                .ForMember(d => d.IsLinkedToProgram,
                    o => o.MapFrom(s => s.LinkedWorkoutProgramID.HasValue));

            // NutritionPlan → NutritionPlanDetailDto
            // IncludeBase pulls in all NutritionPlanCardDto member mappings automatically.
            // CalorieStrategy is the enum-as-string for the strategy type.
            // NutritionWeeks is mapped via the NutritionWeekSummaryDto map below.
            CreateMap<NutritionPlan, NutritionPlanDetailDto>()
                .IncludeBase<NutritionPlan, NutritionPlanCardDto>()
                .ForMember(d => d.CalorieStrategy,
                    o => o.MapFrom(s => s.CalorieStrategyType.ToString()))
                .ForMember(d => d.NutritionWeeks,
                    o => o.MapFrom(s => s.NutritionWeeks));

            // CreateNutritionPlanDto → NutritionPlan  (coach creates plan shell)
            // CoachID is NOT in the DTO — always injected from JWT in the service.
            CreateMap<CreateNutritionPlanDto, NutritionPlan>();

            // UpdateNutritionPlanDto → NutritionPlan  (coach edits plan — full replace)
            // IsPublished and PublishedAt are intentionally absent from UpdateNutritionPlanDto;
            // publish state is controlled exclusively by Publish/Unpublish endpoints.
            CreateMap<UpdateNutritionPlanDto, NutritionPlan>();
        }
    }

    // ── NutritionWeek ──────────────────────────────────────────────────────────

    public class MappingNutritionWeek : Profile
    {
        public MappingNutritionWeek()
        {
            // NutritionWeek → NutritionWeekSummaryDto
            // SessionCount equivalent: DayProtocolCount — derived from the navigation collection.
            // Null-guard on DayProtocols because the collection may not always be eagerly loaded
            // (NutritionPlanWithWeeksSpec does not load DayProtocols).
            CreateMap<NutritionWeek, NutritionWeekSummaryDto>()
                .ForMember(d => d.Id,
                    o => o.MapFrom(s => s.Id))
               .ForMember(d => d.WeekProtocolType,
                    o => o.MapFrom(s => s.WeekProtocolType.ToString()))
                .ForMember(d => d.DayProtocolCount,
                    o => o.MapFrom(s => s.DayProtocols != null ? s.DayProtocols.Count : 0));

            // NutritionWeek → NutritionWeekDetailDto
            // DayProtocols are ordered by service before mapping — AutoMapper maps flat.
            // CoachDirectiveNote is set manually in NutritionEnrollmentService.GetWeekAsync()
            // from the previous week's check-in — not a property on NutritionWeek itself.
            CreateMap<NutritionWeek, NutritionWeekDetailDto>()
                .ForMember(d => d.WeekProtocolType,
                    o => o.MapFrom(s => s.WeekProtocolType.ToString()))
                .ForMember(d => d.IsUnlocked,
                    o => o.Ignore())  // set by service after gate check
                .ForMember(d => d.CoachDirectiveNote,
                    o => o.Ignore())  // set by service from previous check-in
                .ForMember(d => d.DayProtocols,
                    o => o.MapFrom(s => s.DayProtocols));

            // Coach/admin-facing full detail — no unlock gate, no coach-note lookup.
            CreateMap<NutritionWeek, NutritionWeekCoachDetailDto>()
                .ForMember(d => d.WeekProtocolType,
                    o => o.MapFrom(s => s.WeekProtocolType.ToString()))
                .ForMember(d => d.DayProtocols,
                    o => o.MapFrom(s => s.DayProtocols));
        }
    }

    // ── DayProtocol ────────────────────────────────────────────────────────────

    public class MappingDayProtocol : Profile
    {
        public MappingDayProtocol()
        {
            // DayProtocol → DayProtocolDto
            // Both DayProtocolType and WeekDay are enums stored as integers,
            // exposed as strings so clients display the name without the integer value.
            // Meals are mapped via the MealDto map — ordering is done in the service.
            CreateMap<DayProtocol, DayProtocolDto>()
                .ForMember(d => d.DayProtocolType,
                    o => o.MapFrom(s => s.DayProtocolType.ToString()))
                .ForMember(d => d.WeekDay,
                    o => o.MapFrom(s => s.WeekDay.ToString()))
                .ForMember(d => d.Meals,
                    o => o.MapFrom(s => s.Meals));
        }
    }

    // ── Meal ───────────────────────────────────────────────────────────────────

    public class MappingMeal : Profile
    {
        public MappingMeal()
        {
            // Meal → MealDto
            // TimingType exposed as string.
            // FoodItems mapped via MealFoodItemDto — includes calculated macros.
            CreateMap<Meal, MealDto>()
                .ForMember(d => d.TimingType,
                    o => o.MapFrom(s => s.TimingType.ToString()))
                .ForMember(d => d.FoodItems,
                    o => o.MapFrom(s => s.MealFoodItems));

            // MealFoodItem → MealFoodItemDto
            // FoodName traverses FoodItem navigation property.
            // Category is the enum-as-string from the FoodItem.
            // Calculated macros are derived from AmountGrams × per-100g values ÷ 100.
            CreateMap<MealFoodItem, MealFoodItemDto>()
                .ForMember(d => d.FoodName,
                    o => o.MapFrom(s => s.FoodItem.Name))
                .ForMember(d => d.Category,
                    o => o.MapFrom(s => s.FoodItem.Category.ToString()))
                .ForMember(d => d.MacroCalories,
                    o => o.MapFrom(s =>
                        (int)Math.Round(s.AmountGrams / 100m * s.FoodItem.CaloriesPer100g)))
                .ForMember(d => d.MacroProteinG,
                    o => o.MapFrom(s =>
                        (int)Math.Round(s.AmountGrams / 100m * s.FoodItem.ProteinPer100g)))
                .ForMember(d => d.MacroCarbG,
                    o => o.MapFrom(s =>
                        (int)Math.Round(s.AmountGrams / 100m * s.FoodItem.CarbPer100g)))
                .ForMember(d => d.MacroFatG,
                    o => o.MapFrom(s =>
                        (int)Math.Round(s.AmountGrams / 100m * s.FoodItem.FatPer100g)));
        }
    }

    // ── FoodItem ───────────────────────────────────────────────────────────────

    public class MappingFoodItem : Profile
    {
        public MappingFoodItem()
        {
            // FoodItem → FoodItemSummaryDto
            // Category is the enum-as-string.
            // IsGlobal derived from CoachID: null = global (admin-seeded, read-only).
            CreateMap<FoodItem, FoodItemSummaryDto>()
                .ForMember(d => d.Category,
                    o => o.MapFrom(s => s.Category.ToString()))
                .ForMember(d => d.IsGlobal,
                    o => o.MapFrom(s => s.CoachID == null));

            // FoodItem → FoodItemDetailDto
            // IncludeBase pulls in all FoodItemSummaryDto mappings (Category, IsGlobal).
            CreateMap<FoodItem, FoodItemDetailDto>()
                .IncludeBase<FoodItem, FoodItemSummaryDto>();

            // CreateFoodItemDto → FoodItem  (coach creates private food item)
            // CoachID is NOT in the DTO — always injected from JWT in the service.
            CreateMap<CreateFoodItemDto, FoodItem>();

            // FoodItem → CreateFoodItemDto  (ReverseMap used in UpdateFoodItemAsync:
            // _mapper.Map(dto, existingEntity) patches fields, leaving CoachID and ID intact.)
            CreateMap<CreateFoodItemDto, FoodItem>().ReverseMap();
        }
    }

    // ── ClientNutritionConstraints ─────────────────────────────────────────────

    public class MappingConstraints : Profile
    {
        public MappingConstraints()
        {
            // ClientNutritionConstraints → ConstraintsDto
            // All fields are direct matches — no ForMember overrides needed.
            CreateMap<ClientNutritionConstraints, ConstraintsDto>();

            // SetConstraintsDto → ClientNutritionConstraints  (create new)
            CreateMap<SetConstraintsDto, ClientNutritionConstraints>();

            // ClientNutritionConstraints → SetConstraintsDto  (patch existing in UpsertConstraintsAsync)
            CreateMap<SetConstraintsDto, ClientNutritionConstraints>().ReverseMap();
        }
    }
}
