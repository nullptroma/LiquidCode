using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class MergeContestWindows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_contests_available_from",
                table: "contests");

            migrationBuilder.DropIndex(
                name: "ix_contests_available_until",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "available_from",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "available_until",
                table: "contests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "available_from",
                table: "contests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "available_until",
                table: "contests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_contests_available_from",
                table: "contests",
                column: "available_from");

            migrationBuilder.CreateIndex(
                name: "ix_contests_available_until",
                table: "contests",
                column: "available_until");
        }
    }
}
