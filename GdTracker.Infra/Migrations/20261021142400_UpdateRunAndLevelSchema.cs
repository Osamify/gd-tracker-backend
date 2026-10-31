using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTracker.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRunAndLevelSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedAt",
                table: "Runs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Count",
                table: "Runs",
                newName: "TotalAttempts");

            migrationBuilder.AddColumn<float>(
                name: "RunTime",
                table: "Runs",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RunTime",
                table: "Runs");

            migrationBuilder.RenameColumn(
                name: "TotalAttempts",
                table: "Runs",
                newName: "Count");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Runs",
                newName: "LastUpdatedAt");
        }
    }
}
