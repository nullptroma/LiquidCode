using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedDatabaseStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_missions_text_data_mission_id_language",
                table: "missions_text_data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "user_submits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "user_submits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "user_submits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user_submits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "solutions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "solutions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "missions_text_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "missions_text_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "missions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "missions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_deleted",
                table: "users",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_created_at",
                table: "user_submits",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_is_deleted",
                table: "user_submits",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_solutions_created_at",
                table: "solutions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_solutions_status",
                table: "solutions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires",
                table: "refresh_tokens",
                column: "expires");

            migrationBuilder.CreateIndex(
                name: "ix_missions_text_data_language",
                table: "missions_text_data",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "ix_missions_text_data_mission_id_language",
                table: "missions_text_data",
                columns: new[] { "mission_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_missions_created_at",
                table: "missions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_missions_difficulty",
                table: "missions",
                column: "difficulty");

            migrationBuilder.CreateIndex(
                name: "ix_missions_is_deleted",
                table: "missions",
                column: "is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_is_deleted",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_user_submits_created_at",
                table: "user_submits");

            migrationBuilder.DropIndex(
                name: "ix_user_submits_is_deleted",
                table: "user_submits");

            migrationBuilder.DropIndex(
                name: "ix_solutions_created_at",
                table: "solutions");

            migrationBuilder.DropIndex(
                name: "ix_solutions_status",
                table: "solutions");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_expires",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "ix_missions_text_data_language",
                table: "missions_text_data");

            migrationBuilder.DropIndex(
                name: "ix_missions_text_data_mission_id_language",
                table: "missions_text_data");

            migrationBuilder.DropIndex(
                name: "ix_missions_created_at",
                table: "missions");

            migrationBuilder.DropIndex(
                name: "ix_missions_difficulty",
                table: "missions");

            migrationBuilder.DropIndex(
                name: "ix_missions_is_deleted",
                table: "missions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "user_submits");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_submits");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_submits");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user_submits");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "missions_text_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "missions_text_data");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "missions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "missions");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "ix_missions_text_data_mission_id_language",
                table: "missions_text_data",
                columns: new[] { "mission_id", "language" });
        }
    }
}
