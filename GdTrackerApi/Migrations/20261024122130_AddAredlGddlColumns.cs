using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAredlGddlColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NlwTier",
                table: "Levels",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NlwTier",
                table: "Levels");
        }
    }
}
