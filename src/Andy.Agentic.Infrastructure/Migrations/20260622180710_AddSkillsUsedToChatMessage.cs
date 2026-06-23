using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Andy.Agentic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillsUsedToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SkillsUsed",
                table: "ChatMessages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillsUsed",
                table: "ChatMessages");
        }
    }
}
