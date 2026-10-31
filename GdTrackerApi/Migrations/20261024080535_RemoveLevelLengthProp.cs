using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLevelLengthProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelLength",
                table: "Levels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LevelLength",
                table: "Levels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
