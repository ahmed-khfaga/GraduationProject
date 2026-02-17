using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTablesAndAddAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeProgramTemplates_ProgramTemplates_ProgramTemplateID",
                table: "TraineeProgramTemplates");

            migrationBuilder.DropTable(
                name: "ProgramDays");

            migrationBuilder.DropTable(
                name: "ProgramTemplates");

            migrationBuilder.DropTable(
                name: "BasePrograms");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProgramTemplates_ProgramTemplateID",
                table: "TraineeProgramTemplates");

            migrationBuilder.AddColumn<int>(
                name: "WorkoutProgramID",
                table: "TraineeProgramTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPrograms",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackID = table.Column<int>(type: "int", nullable: false),
                    CoachID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationOnWeeks = table.Column<int>(type: "int", nullable: false),
                    SessionsPerWeeks = table.Column<int>(type: "int", nullable: false),
                    SessionsDuration = table.Column<int>(type: "int", nullable: false),
                    PhotoThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrainingGoal = table.Column<int>(type: "int", nullable: false),
                    FitnessLevel = table.Column<int>(type: "int", nullable: false),
                    EquipmentType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPrograms", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WorkoutPrograms_Coachs_CoachID",
                        column: x => x.CoachID,
                        principalTable: "Coachs",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_WorkoutPrograms_Tracks_TrackID",
                        column: x => x.TrackID,
                        principalTable: "Tracks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramWeek",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutProgramID = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    WeekDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FocusArea = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramWeek", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProgramWeek_WorkoutPrograms_WorkoutProgramID",
                        column: x => x.WorkoutProgramID,
                        principalTable: "WorkoutPrograms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramWeekID = table.Column<int>(type: "int", nullable: false),
                    SessionTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    weekDay = table.Column<int>(type: "int", nullable: false),
                    EstimatedDuration = table.Column<int>(type: "int", nullable: false),
                    WarmupNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CooldownNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_ProgramWeek_ProgramWeekID",
                        column: x => x.ProgramWeekID,
                        principalTable: "ProgramWeek",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramTemplates_WorkoutProgramID",
                table: "TraineeProgramTemplates",
                column: "WorkoutProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramWeek_WorkoutProgramID",
                table: "ProgramWeek",
                column: "WorkoutProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPrograms_CoachID",
                table: "WorkoutPrograms",
                column: "CoachID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPrograms_TrackID",
                table: "WorkoutPrograms",
                column: "TrackID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_ProgramWeekID",
                table: "WorkoutSessions",
                column: "ProgramWeekID");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeProgramTemplates_WorkoutPrograms_WorkoutProgramID",
                table: "TraineeProgramTemplates",
                column: "WorkoutProgramID",
                principalTable: "WorkoutPrograms",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeProgramTemplates_WorkoutPrograms_WorkoutProgramID",
                table: "TraineeProgramTemplates");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "ProgramWeek");

            migrationBuilder.DropTable(
                name: "WorkoutPrograms");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProgramTemplates_WorkoutProgramID",
                table: "TraineeProgramTemplates");

            migrationBuilder.DropColumn(
                name: "WorkoutProgramID",
                table: "TraineeProgramTemplates");

            migrationBuilder.CreateTable(
                name: "BasePrograms",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePrograms", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProgramTemplates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseProgramID = table.Column<int>(type: "int", nullable: false),
                    CoachID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramTemplates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProgramTemplates_BasePrograms_BaseProgramID",
                        column: x => x.BaseProgramID,
                        principalTable: "BasePrograms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramTemplates_Coachs_CoachID",
                        column: x => x.CoachID,
                        principalTable: "Coachs",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ProgramDays",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramTemplateID = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Focus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramDays", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProgramDays_ProgramTemplates_ProgramTemplateID",
                        column: x => x.ProgramTemplateID,
                        principalTable: "ProgramTemplates",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgramTemplates_ProgramTemplateID",
                table: "TraineeProgramTemplates",
                column: "ProgramTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramDays_ProgramTemplateID",
                table: "ProgramDays",
                column: "ProgramTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTemplates_BaseProgramID",
                table: "ProgramTemplates",
                column: "BaseProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTemplates_CoachID",
                table: "ProgramTemplates",
                column: "CoachID");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeProgramTemplates_ProgramTemplates_ProgramTemplateID",
                table: "TraineeProgramTemplates",
                column: "ProgramTemplateID",
                principalTable: "ProgramTemplates",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
