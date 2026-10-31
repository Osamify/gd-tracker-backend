using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTracker.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddGddpThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tier",
                table: "Levels",
                newName: "GddpTier");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Levels",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Levels");

            migrationBuilder.RenameColumn(
                name: "GddpTier",
                table: "Levels",
                newName: "Tier");
        }
    }
}
