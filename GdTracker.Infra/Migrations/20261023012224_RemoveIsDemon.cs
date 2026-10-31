using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTracker.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsDemon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDemon",
                table: "Levels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemon",
                table: "Levels",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
