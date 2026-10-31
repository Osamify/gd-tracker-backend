using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelDifficultyAndTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Levels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Tier",
                table: "Levels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "Tier",
                table: "Levels");
        }
    }
}
