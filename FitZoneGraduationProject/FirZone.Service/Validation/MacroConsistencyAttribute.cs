using FitZone.Service.DTOs.NutritionDTOs;
using System.ComponentModel.DataAnnotations;

namespace FitZone.Service.Validation
{
    /// <summary>
    /// Validates that the macro breakdown (protein × 4 + carb × 4 + fat × 9) sums
    /// to within ±50 kcal of TotalCaloriesTarget.
    ///
    /// Why ±50 kcal tolerance?
    ///   Macros must be whole grams (integers) so rounding is unavoidable.
    ///   50 kcal is the smallest gap that cannot be eliminated without using
    ///   fractional gram inputs, which are impractical.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MacroConsistencyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is not CreateDayProtocolDto dto)
                return ValidationResult.Success;

            int macroKcal = (dto.ProteinTargetG * 4)
                          + (dto.CarbTargetG * 4)
                          + (dto.FatTargetG * 9);

            int gap = Math.Abs(macroKcal - dto.TotalCaloriesTarget);

            if (gap > 50)
                return new ValidationResult(
                    $"Macro breakdown sums to {macroKcal} kcal but " +
                    $"TotalCaloriesTarget is {dto.TotalCaloriesTarget} kcal " +
                    $"(gap: {gap} kcal). Adjust protein, carbs, or fat so the " +
                    $"difference is within ±50 kcal. " +
                    $"Calculation: protein({dto.ProteinTargetG}g×4) + " +
                    $"carbs({dto.CarbTargetG}g×4) + fat({dto.FatTargetG}g×9) = {macroKcal} kcal.",
                    new[] { nameof(dto.TotalCaloriesTarget) });

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Variant for UpdateDayProtocolDto where all macro fields are nullable.
    /// Validates only when at least one macro field is provided.
    /// If all macro fields are null (notes-only update), validation is skipped entirely.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MacroConsistencyOnUpdateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is not UpdateDayProtocolDto dto)
                return ValidationResult.Success;

            bool anyMacroSet = dto.TotalCaloriesTarget.HasValue
                            || dto.ProteinTargetG.HasValue
                            || dto.CarbTargetG.HasValue
                            || dto.FatTargetG.HasValue;

            if (!anyMacroSet) return ValidationResult.Success;

            // If any macro field is provided, all four must be provided together.
            if (!dto.TotalCaloriesTarget.HasValue || !dto.ProteinTargetG.HasValue
             || !dto.CarbTargetG.HasValue || !dto.FatTargetG.HasValue)
            {
                return new ValidationResult(
                    "When updating macro targets, all four fields must be provided together: " +
                    "TotalCaloriesTarget, ProteinTargetG, CarbTargetG, and FatTargetG.",
                    new[] { nameof(dto.TotalCaloriesTarget) });
            }

            int macroKcal = (dto.ProteinTargetG!.Value * 4)
                          + (dto.CarbTargetG!.Value * 4)
                          + (dto.FatTargetG!.Value * 9);

            int gap = Math.Abs(macroKcal - dto.TotalCaloriesTarget!.Value);

            if (gap > 50)
                return new ValidationResult(
                    $"Macro breakdown sums to {macroKcal} kcal but TotalCaloriesTarget " +
                    $"is {dto.TotalCaloriesTarget.Value} kcal (gap: {gap} kcal). " +
                    $"Adjust macros so the difference is within ±50 kcal.",
                    new[] { nameof(dto.TotalCaloriesTarget) });

            return ValidationResult.Success;
        }
    }
}