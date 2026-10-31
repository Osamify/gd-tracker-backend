using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdTracker.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddAredlAndLastPlayedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AredlRankWhenCompleted",
                table: "Levels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentAredlRank",
                table: "Levels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayedAt",
                table: "Levels",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AredlRankWhenCompleted",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "CurrentAredlRank",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "LastPlayedAt",
                table: "Levels");
        }
    }
}
