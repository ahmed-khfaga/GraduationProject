using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovalWorkflow_CoachExercises_RicherWeeks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TraineeProgramEnrollments_TraineeID_IsActive",
                table: "TraineeProgramEnrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkoutPrograms");

            migrationBuilder.RenameColumn(
                name: "RejectionNote",
                table: "WorkoutPrograms",
                newName: "NextSteps");

            migrationBuilder.AddColumn<int>(
                name: "DayOrder",
                table: "WorkoutSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedOutcome",
                table: "WorkoutPrograms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "WorkoutPrograms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NextWeekPreview",
                table: "ProgramWeeks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressionNote",
                table: "ProgramWeeks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoachID",
                table: "Exercises",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramEnrollments_TraineeID_TrackID_IsActive",
                table: "TraineeProgramEnrollments",
                columns: new[] { "TraineeID", "TrackID", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CoachID",
                table: "Exercises",
                column: "CoachID");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Coachs_CoachID",
                table: "Exercises",
                column: "CoachID",
                principalTable: "Coachs",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Coachs_CoachID",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProgramEnrollments_TraineeID_TrackID_IsActive",
                table: "TraineeProgramEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CoachID",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "DayOrder",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "ExpectedOutcome",
                table: "WorkoutPrograms");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "WorkoutPrograms");

            migrationBuilder.DropColumn(
                name: "NextWeekPreview",
                table: "ProgramWeeks");

            migrationBuilder.DropColumn(
                name: "ProgressionNote",
                table: "ProgramWeeks");

            migrationBuilder.DropColumn(
                name: "CoachID",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "NextSteps",
                table: "WorkoutPrograms",
                newName: "RejectionNote");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WorkoutPrograms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramEnrollments_TraineeID_IsActive",
                table: "TraineeProgramEnrollments",
                columns: new[] { "TraineeID", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");
        }
    }
}
