using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class ContestSchedulingModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "starts_at",
                table: "contests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ends_at",
                table: "contests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "attempt_duration_minutes",
                table: "contests",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "schedule_type",
                table: "contests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "active_attempt_expires_at",
                table: "contest_memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "active_attempt_started_at",
                table: "contest_memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "attempt_count",
                table: "contest_memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_contests_available_from",
                table: "contests",
                column: "available_from");

            migrationBuilder.CreateIndex(
                name: "ix_contests_available_until",
                table: "contests",
                column: "available_until");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_contests_available_from",
                table: "contests");

            migrationBuilder.DropIndex(
                name: "ix_contests_available_until",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "attempt_duration_minutes",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "available_from",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "available_until",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "schedule_type",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "active_attempt_expires_at",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "active_attempt_started_at",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "attempt_count",
                table: "contest_memberships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "starts_at",
                table: "contests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ends_at",
                table: "contests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
