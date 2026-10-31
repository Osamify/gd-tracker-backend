using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTracker.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coins",
                table: "Levels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Coins",
                table: "Levels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
