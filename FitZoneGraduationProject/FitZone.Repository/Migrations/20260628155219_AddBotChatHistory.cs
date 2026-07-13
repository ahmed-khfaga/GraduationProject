using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddBotChatHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "ChatMessages",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "BotConversationId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BotRole",
                table: "ChatMessages",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChatType",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_BotSession",
                table: "ChatMessages",
                columns: new[] { "SenderId", "BotConversationId" },
                filter: "[BotConversationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_HumanChat",
                table: "ChatMessages",
                columns: new[] { "SenderId", "ReceiverId", "ChatType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_BotSession",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_HumanChat",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "BotConversationId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "BotRole",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ChatType",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "ChatMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");
        }
    }
}
