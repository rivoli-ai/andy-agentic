using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Andy.Agentic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImagesToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: production DBs may already have "Images" (manual patch or partial run)
            // while __EFMigrationsHistory still lacks this migration id.
            migrationBuilder.Sql(
                """
                ALTER TABLE "ChatMessages" ADD COLUMN IF NOT EXISTS "Images" text NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "ChatMessages" DROP COLUMN IF EXISTS "Images";
                """);
        }
    }
}
