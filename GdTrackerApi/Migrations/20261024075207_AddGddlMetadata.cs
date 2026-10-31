using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGddlMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Levels");

            migrationBuilder.AddColumn<double>(
                name: "Enjoyment",
                table: "Levels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Levels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShowcaseVideoId",
                table: "Levels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSeconds",
                table: "Levels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enjoyment",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "ShowcaseVideoId",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "TotalSeconds",
                table: "Levels");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Levels",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
