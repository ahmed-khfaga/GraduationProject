using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class InheritFromBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeMemberships_Memberships_MembershipId",
                table: "TraineeMemberships");

            migrationBuilder.RenameColumn(
                name: "MembershipId",
                table: "TraineeMemberships",
                newName: "MembershipID");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeMemberships_MembershipId",
                table: "TraineeMemberships",
                newName: "IX_TraineeMemberships_MembershipID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Memberships",
                newName: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeMemberships_Memberships_MembershipID",
                table: "TraineeMemberships",
                column: "MembershipID",
                principalTable: "Memberships",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeMemberships_Memberships_MembershipID",
                table: "TraineeMemberships");

            migrationBuilder.RenameColumn(
                name: "MembershipID",
                table: "TraineeMemberships",
                newName: "MembershipId");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeMemberships_MembershipID",
                table: "TraineeMemberships",
                newName: "IX_TraineeMemberships_MembershipId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Memberships",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeMemberships_Memberships_MembershipId",
                table: "TraineeMemberships",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id");
        }
    }
}
