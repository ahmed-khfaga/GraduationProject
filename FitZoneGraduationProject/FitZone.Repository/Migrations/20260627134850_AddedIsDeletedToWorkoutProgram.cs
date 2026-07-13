using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsDeletedToWorkoutProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WorkoutPrograms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WorkoutPrograms");
        }
    }
}
