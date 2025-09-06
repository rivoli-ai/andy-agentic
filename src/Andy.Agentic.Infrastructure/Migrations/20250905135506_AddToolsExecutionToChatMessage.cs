using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Andy.Agentic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddToolsExecutionToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatMessageEntityId",
                table: "ToolExecutionLogs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ToolExecutionLogs_ChatMessageEntityId",
                table: "ToolExecutionLogs",
                column: "ChatMessageEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToolExecutionLogs_ChatMessages_ChatMessageEntityId",
                table: "ToolExecutionLogs",
                column: "ChatMessageEntityId",
                principalTable: "ChatMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToolExecutionLogs_ChatMessages_ChatMessageEntityId",
                table: "ToolExecutionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ToolExecutionLogs_ChatMessageEntityId",
                table: "ToolExecutionLogs");

            migrationBuilder.DropColumn(
                name: "ChatMessageEntityId",
                table: "ToolExecutionLogs");
        }
    }
}
