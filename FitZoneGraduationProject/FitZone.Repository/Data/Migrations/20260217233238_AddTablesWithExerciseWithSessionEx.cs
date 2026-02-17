using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesWithExerciseWithSessionEx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgramWeek_WorkoutPrograms_WorkoutProgramID",
                table: "ProgramWeek");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_ProgramWeek_ProgramWeekID",
                table: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "TraineeProgramTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramWeek",
                table: "ProgramWeek");

            migrationBuilder.RenameTable(
                name: "ProgramWeek",
                newName: "ProgramWeeks");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramWeek_WorkoutProgramID",
                table: "ProgramWeeks",
                newName: "IX_ProgramWeeks_WorkoutProgramID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramWeeks",
                table: "ProgramWeeks",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryMuscles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryMuscles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EquipmentNeeded = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FitnessLevel = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommonMistakes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TraineeProgramEnrollments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeID = table.Column<int>(type: "int", nullable: false),
                    WorkoutProgramID = table.Column<int>(type: "int", nullable: false),
                    TrackID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentWeekNumber = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeProgramEnrollments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TraineeProgramEnrollments_Tracks_TrackID",
                        column: x => x.TrackID,
                        principalTable: "Tracks",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TraineeProgramEnrollments_Trainees_TraineeID",
                        column: x => x.TraineeID,
                        principalTable: "Trainees",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TraineeProgramEnrollments_WorkoutPrograms_WorkoutProgramID",
                        column: x => x.WorkoutProgramID,
                        principalTable: "WorkoutPrograms",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "SessionExercises",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutSessionID = table.Column<int>(type: "int", nullable: false),
                    ExerciseID = table.Column<int>(type: "int", nullable: false),
                    SectionType = table.Column<int>(type: "int", nullable: false),
                    Sets = table.Column<int>(type: "int", nullable: true),
                    Reps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RestSeconds = table.Column<int>(type: "int", nullable: true),
                    Tempo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RPETarget = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionExercises", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SessionExercises_Exercises_ExerciseID",
                        column: x => x.ExerciseID,
                        principalTable: "Exercises",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionExercises_WorkoutSessions_WorkoutSessionID",
                        column: x => x.WorkoutSessionID,
                        principalTable: "WorkoutSessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercises_ExerciseID",
                table: "SessionExercises",
                column: "ExerciseID");

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercises_WorkoutSessionID",
                table: "SessionExercises",
                column: "WorkoutSessionID");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramEnrollments_TrackID",
                table: "TraineeProgramEnrollments",
                column: "TrackID");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramEnrollments_TraineeID_IsActive",
                table: "TraineeProgramEnrollments",
                columns: new[] { "TraineeID", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramEnrollments_WorkoutProgramID",
                table: "TraineeProgramEnrollments",
                column: "WorkoutProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramWeeks_WorkoutPrograms_WorkoutProgramID",
                table: "ProgramWeeks",
                column: "WorkoutProgramID",
                principalTable: "WorkoutPrograms",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_ProgramWeeks_ProgramWeekID",
                table: "WorkoutSessions",
                column: "ProgramWeekID",
                principalTable: "ProgramWeeks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgramWeeks_WorkoutPrograms_WorkoutProgramID",
                table: "ProgramWeeks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_ProgramWeeks_ProgramWeekID",
                table: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "SessionExercises");

            migrationBuilder.DropTable(
                name: "TraineeProgramEnrollments");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramWeeks",
                table: "ProgramWeeks");

            migrationBuilder.RenameTable(
                name: "ProgramWeeks",
                newName: "ProgramWeek");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramWeeks_WorkoutProgramID",
                table: "ProgramWeek",
                newName: "IX_ProgramWeek_WorkoutProgramID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramWeek",
                table: "ProgramWeek",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "TraineeProgramTemplates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeID = table.Column<int>(type: "int", nullable: false),
                    WorkoutProgramID = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProgramTemplateID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeProgramTemplates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TraineeProgramTemplates_Trainees_TraineeID",
                        column: x => x.TraineeID,
                        principalTable: "Trainees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TraineeProgramTemplates_WorkoutPrograms_WorkoutProgramID",
                        column: x => x.WorkoutProgramID,
                        principalTable: "WorkoutPrograms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramTemplates_TraineeID_IsActive",
                table: "TraineeProgramTemplates",
                columns: new[] { "TraineeID", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramTemplates_WorkoutProgramID",
                table: "TraineeProgramTemplates",
                column: "WorkoutProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramWeek_WorkoutPrograms_WorkoutProgramID",
                table: "ProgramWeek",
                column: "WorkoutProgramID",
                principalTable: "WorkoutPrograms",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_ProgramWeek_ProgramWeekID",
                table: "WorkoutSessions",
                column: "ProgramWeekID",
                principalTable: "ProgramWeek",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
