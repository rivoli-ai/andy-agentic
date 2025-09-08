using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Andy.Agentic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Tools",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Tools",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "LlmConfigs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "LlmConfigs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Agents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Agents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tools_CreatedByUserId",
                table: "Tools",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LlmConfigs_CreatedByUserId",
                table: "LlmConfigs",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LlmConfigs_Users_CreatedByUserId",
                table: "LlmConfigs",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tools_Users_CreatedByUserId",
                table: "Tools",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LlmConfigs_Users_CreatedByUserId",
                table: "LlmConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tools_Users_CreatedByUserId",
                table: "Tools");

            migrationBuilder.DropIndex(
                name: "IX_Tools_CreatedByUserId",
                table: "Tools");

            migrationBuilder.DropIndex(
                name: "IX_LlmConfigs_CreatedByUserId",
                table: "LlmConfigs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LlmConfigs");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "LlmConfigs");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Agents");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Agents",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
