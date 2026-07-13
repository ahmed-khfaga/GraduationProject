using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CaloriesPer100g = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    ProteinPer100g = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    CarbPer100g = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    FatPer100g = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    FiberPer100g = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    ServingSizeG = table.Column<int>(type: "int", nullable: false),
                    ServingSizeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsWhole = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodItems_Coachs_CoachID",
                        column: x => x.CoachID,
                        principalTable: "Coachs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NutritionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachID = table.Column<int>(type: "int", nullable: false),
                    LinkedWorkoutProgramID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedOutcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrainingGoal = table.Column<int>(type: "int", nullable: false),
                    FitnessLevel = table.Column<int>(type: "int", nullable: false),
                    EquipmentType = table.Column<int>(type: "int", nullable: false),
                    DurationOnWeeks = table.Column<int>(type: "int", nullable: false),
                    CalorieStrategyType = table.Column<int>(type: "int", nullable: false),
                    TDEEAdjustmentKcal = table.Column<int>(type: "int", nullable: true),
                    AbsoluteCalorieTarget = table.Column<int>(type: "int", nullable: true),
                    ProteinTargetPerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhotoThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionPlans_Coachs_CoachID",
                        column: x => x.CoachID,
                        principalTable: "Coachs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NutritionPlans_WorkoutPrograms_LinkedWorkoutProgramID",
                        column: x => x.LinkedWorkoutProgramID,
                        principalTable: "WorkoutPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NutritionWeeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NutritionPlanID = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    WeekProtocolType = table.Column<int>(type: "int", nullable: false),
                    CalorieModifier = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    WeekDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FocusNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextWeekPreview = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionWeeks_NutritionPlans_NutritionPlanID",
                        column: x => x.NutritionPlanID,
                        principalTable: "NutritionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraineeNutritionEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeID = table.Column<int>(type: "int", nullable: false),
                    NutritionPlanID = table.Column<int>(type: "int", nullable: false),
                    LinkedWorkoutEnrollmentID = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxWeekUnlocked = table.Column<int>(type: "int", nullable: false),
                    BaselineCalories = table.Column<int>(type: "int", nullable: false),
                    CurrentAdjustedKcal = table.Column<int>(type: "int", nullable: false),
                    EmpiricalTDEEKcal = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeNutritionEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraineeNutritionEnrollments_NutritionPlans_NutritionPlanID",
                        column: x => x.NutritionPlanID,
                        principalTable: "NutritionPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TraineeNutritionEnrollments_TraineeProgramEnrollments_LinkedWorkoutEnrollmentID",
                        column: x => x.LinkedWorkoutEnrollmentID,
                        principalTable: "TraineeProgramEnrollments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TraineeNutritionEnrollments_Trainees_TraineeID",
                        column: x => x.TraineeID,
                        principalTable: "Trainees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DayProtocols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NutritionWeekID = table.Column<int>(type: "int", nullable: false),
                    DayProtocolType = table.Column<int>(type: "int", nullable: false),
                    WeekDay = table.Column<int>(type: "int", nullable: false),
                    DayOrder = table.Column<int>(type: "int", nullable: false),
                    LinkedWorkoutSessionID = table.Column<int>(type: "int", nullable: true),
                    TotalCaloriesTarget = table.Column<int>(type: "int", nullable: false),
                    ProteinTargetG = table.Column<int>(type: "int", nullable: false),
                    CarbTargetG = table.Column<int>(type: "int", nullable: false),
                    FatTargetG = table.Column<int>(type: "int", nullable: false),
                    ProtocolNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayProtocols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayProtocols_NutritionWeeks_NutritionWeekID",
                        column: x => x.NutritionWeekID,
                        principalTable: "NutritionWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayProtocols_WorkoutSessions_LinkedWorkoutSessionID",
                        column: x => x.LinkedWorkoutSessionID,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClientNutritionConstraints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentID = table.Column<int>(type: "int", nullable: false),
                    WeightAveragingDays = table.Column<int>(type: "int", nullable: false),
                    ExpectedWeeklyChangeMin = table.Column<decimal>(type: "decimal(5,3)", nullable: false),
                    ExpectedWeeklyChangeMax = table.Column<decimal>(type: "decimal(5,3)", nullable: false),
                    DeviationTriggerKg = table.Column<decimal>(type: "decimal(5,3)", nullable: false),
                    ProteinFloorG = table.Column<int>(type: "int", nullable: false),
                    FatFloorG = table.Column<int>(type: "int", nullable: false),
                    CalorieFloor = table.Column<int>(type: "int", nullable: false),
                    CalorieCeiling = table.Column<int>(type: "int", nullable: false),
                    MaxSingleAdjustmentKcal = table.Column<int>(type: "int", nullable: false),
                    MaxCumulativeDriftKcal = table.Column<int>(type: "int", nullable: false),
                    PreferredAdjustmentVector = table.Column<int>(type: "int", nullable: false),
                    AdherenceThresholdPercent = table.Column<int>(type: "int", nullable: false),
                    RequireConsecutiveWeeksDeviation = table.Column<bool>(type: "bit", nullable: false),
                    ApplyTrainingWeekNoiseCorrection = table.Column<bool>(type: "bit", nullable: false),
                    EnergyLevelEscalationRule = table.Column<bool>(type: "bit", nullable: false),
                    PreserveLeanMassOverRate = table.Column<bool>(type: "bit", nullable: false),
                    EnableBaselineRecalibrationReview = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientNutritionConstraints", x => x.Id);
                    table.CheckConstraint("CK_Constraints_Adherence", "[AdherenceThresholdPercent] >= 0 AND [AdherenceThresholdPercent] <= 100");
                    table.CheckConstraint("CK_Constraints_WeightAveragingDays", "[WeightAveragingDays] IN (3, 5, 7)");
                    table.ForeignKey(
                        name: "FK_ClientNutritionConstraints_TraineeNutritionEnrollments_EnrollmentID",
                        column: x => x.EnrollmentID,
                        principalTable: "TraineeNutritionEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyCheckIns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentID = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MorningWeight1 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    MorningWeight2 = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MorningWeight3 = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    AverageWeight = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    EnergyLevel = table.Column<int>(type: "int", nullable: false),
                    HungerLevel = table.Column<int>(type: "int", nullable: false),
                    SleepQuality = table.Column<int>(type: "int", nullable: false),
                    AdherencePercent = table.Column<int>(type: "int", nullable: false),
                    ClientNote = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    NoteCategory = table.Column<int>(type: "int", nullable: true),
                    SystemProposalKcal = table.Column<int>(type: "int", nullable: true),
                    SystemProposalReasoning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectedOutcomeIfNoAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemConfidence = table.Column<int>(type: "int", nullable: false),
                    CoachDecision = table.Column<int>(type: "int", nullable: true),
                    FinalAdjustmentKcal = table.Column<int>(type: "int", nullable: true),
                    AppliedAdjustmentVector = table.Column<int>(type: "int", nullable: true),
                    CoachNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CoachNoteAction = table.Column<int>(type: "int", nullable: true),
                    CoachReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoachApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyCheckIns", x => x.Id);
                    table.CheckConstraint("CK_CheckIn_Adherence", "[AdherencePercent] >= 0 AND [AdherencePercent] <= 100");
                    table.CheckConstraint("CK_CheckIn_EnergyLevel", "[EnergyLevel] >= 1 AND [EnergyLevel] <= 5");
                    table.CheckConstraint("CK_CheckIn_HungerLevel", "[HungerLevel] >= 1 AND [HungerLevel] <= 5");
                    table.CheckConstraint("CK_CheckIn_SleepQuality", "[SleepQuality] >= 1 AND [SleepQuality] <= 5");
                    table.ForeignKey(
                        name: "FK_WeeklyCheckIns_TraineeNutritionEnrollments_EnrollmentID",
                        column: x => x.EnrollmentID,
                        principalTable: "TraineeNutritionEnrollments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayProtocolID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimingType = table.Column<int>(type: "int", nullable: false),
                    MealOrder = table.Column<int>(type: "int", nullable: false),
                    TimeFromTrainingMinutes = table.Column<int>(type: "int", nullable: true),
                    TargetCalories = table.Column<int>(type: "int", nullable: false),
                    TargetProteinG = table.Column<int>(type: "int", nullable: false),
                    TargetCarbG = table.Column<int>(type: "int", nullable: false),
                    TargetFatG = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meals_DayProtocols_DayProtocolID",
                        column: x => x.DayProtocolID,
                        principalTable: "DayProtocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealFoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MealID = table.Column<int>(type: "int", nullable: false),
                    FoodItemID = table.Column<int>(type: "int", nullable: false),
                    AmountGrams = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    SwapGroupID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealFoodItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealFoodItems_FoodItems_FoodItemID",
                        column: x => x.FoodItemID,
                        principalTable: "FoodItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MealFoodItems_Meals_MealID",
                        column: x => x.MealID,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientNutritionConstraints_EnrollmentID",
                table: "ClientNutritionConstraints",
                column: "EnrollmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DayProtocols_LinkedWorkoutSessionID",
                table: "DayProtocols",
                column: "LinkedWorkoutSessionID");

            migrationBuilder.CreateIndex(
                name: "IX_DayProtocols_NutritionWeekID",
                table: "DayProtocols",
                column: "NutritionWeekID");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_CoachID",
                table: "FoodItems",
                column: "CoachID");

            migrationBuilder.CreateIndex(
                name: "IX_MealFoodItems_FoodItemID",
                table: "MealFoodItems",
                column: "FoodItemID");

            migrationBuilder.CreateIndex(
                name: "IX_MealFoodItems_MealID",
                table: "MealFoodItems",
                column: "MealID");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_DayProtocolID",
                table: "Meals",
                column: "DayProtocolID");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlans_CoachID",
                table: "NutritionPlans",
                column: "CoachID");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlans_LinkedWorkoutProgramID",
                table: "NutritionPlans",
                column: "LinkedWorkoutProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionWeeks_NutritionPlanID",
                table: "NutritionWeeks",
                column: "NutritionPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeNutritionEnrollments_LinkedWorkoutEnrollmentID",
                table: "TraineeNutritionEnrollments",
                column: "LinkedWorkoutEnrollmentID");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeNutritionEnrollments_NutritionPlanID",
                table: "TraineeNutritionEnrollments",
                column: "NutritionPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeNutritionEnrollments_TraineeID_NutritionPlanID_IsActive",
                table: "TraineeNutritionEnrollments",
                columns: new[] { "TraineeID", "NutritionPlanID", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyCheckIns_EnrollmentID_WeekNumber",
                table: "WeeklyCheckIns",
                columns: new[] { "EnrollmentID", "WeekNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientNutritionConstraints");

            migrationBuilder.DropTable(
                name: "MealFoodItems");

            migrationBuilder.DropTable(
                name: "WeeklyCheckIns");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "Meals");

            migrationBuilder.DropTable(
                name: "TraineeNutritionEnrollments");

            migrationBuilder.DropTable(
                name: "DayProtocols");

            migrationBuilder.DropTable(
                name: "NutritionWeeks");

            migrationBuilder.DropTable(
                name: "NutritionPlans");
        }
    }
}
