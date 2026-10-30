using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsControllers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Difficulty",
                table: "Levels",
                newName: "Rating");

            migrationBuilder.AddColumn<int>(
                name: "Coins",
                table: "Levels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DemonDifficulty",
                table: "Levels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDemon",
                table: "Levels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRated",
                table: "Levels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LevelLength",
                table: "Levels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginalLevelId",
                table: "Levels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coins",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "DemonDifficulty",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "IsDemon",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "IsRated",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "LevelLength",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "OriginalLevelId",
                table: "Levels");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Levels",
                newName: "Difficulty");
        }
    }
}
