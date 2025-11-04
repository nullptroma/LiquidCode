using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active_attempt_expires_at",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "attempt_count",
                table: "contest_memberships");

            migrationBuilder.RenameColumn(
                name: "active_attempt_started_at",
                table: "contest_memberships",
                newName: "last_attempt_started_at");

            migrationBuilder.AddColumn<int>(
                name: "contest_attempt_id",
                table: "user_submits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "invitation_id",
                table: "group_memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "invited_by_id",
                table: "group_memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_auto_joined",
                table: "group_memberships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "joined_at",
                table: "group_memberships",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "allow_early_finish",
                table: "contests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "max_attempts",
                table: "contests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "visibility",
                table: "contests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "active_attempt_id",
                table: "contest_memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "invitation_id",
                table: "contest_memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_auto_joined",
                table: "contest_memberships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "joined_at",
                table: "contest_memberships",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "contest_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    contest_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    attempt_index = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    finished_by = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_score = table.Column<decimal>(type: "numeric", nullable: false),
                    solved_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_contest_attempts_contest_memberships_contest_id_user_id",
                        columns: x => new { x.contest_id, x.user_id },
                        principalTable: "contest_memberships",
                        principalColumns: new[] { "contest_id", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_attempts_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "contests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_invitations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    inviter_id = table.Column<int>(type: "integer", nullable: false),
                    invitee_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    delivery_channel = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    declined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_invitations_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_invitations_users_invitee_id",
                        column: x => x.invitee_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_invitations_users_inviter_id",
                        column: x => x.inviter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_join_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_refreshed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    usage_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_join_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_join_tokens_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_join_tokens_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contest_attempt_mission_results",
                columns: table => new
                {
                    contest_attempt_id = table.Column<int>(type: "integer", nullable: false),
                    mission_id = table.Column<int>(type: "integer", nullable: false),
                    solved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submission_count = table.Column<int>(type: "integer", nullable: false),
                    highest_score = table.Column<decimal>(type: "numeric", nullable: false),
                    penalty = table.Column<double>(type: "double precision", nullable: false),
                    last_submission_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    best_submission_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_attempt_mission_results", x => new { x.contest_attempt_id, x.mission_id });
                    table.ForeignKey(
                        name: "fk_contest_attempt_mission_results_contest_attempts_contest_at",
                        column: x => x.contest_attempt_id,
                        principalTable: "contest_attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_attempt_mission_results_missions_mission_id",
                        column: x => x.mission_id,
                        principalTable: "missions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_attempt_mission_results_user_submits_best_submissio",
                        column: x => x.best_submission_id,
                        principalTable: "user_submits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_contest_attempt_id",
                table: "user_submits",
                column: "contest_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_memberships_invitation_id",
                table: "group_memberships",
                column: "invitation_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_memberships_invited_by_id",
                table: "group_memberships",
                column: "invited_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_contests_visibility",
                table: "contests",
                column: "visibility");

            migrationBuilder.CreateIndex(
                name: "ix_contest_memberships_active_attempt_id",
                table: "contest_memberships",
                column: "active_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_contest_memberships_invitation_id",
                table: "contest_memberships",
                column: "invitation_id");

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempt_mission_results_best_submission_id",
                table: "contest_attempt_mission_results",
                column: "best_submission_id");

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempt_mission_results_mission_id",
                table: "contest_attempt_mission_results",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempts_contest_id_user_id_attempt_index",
                table: "contest_attempts",
                columns: new[] { "contest_id", "user_id", "attempt_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempts_expires_at",
                table: "contest_attempts",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempts_started_at",
                table: "contest_attempts",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempts_status",
                table: "contest_attempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_contest_attempts_user_id",
                table: "contest_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_invitations_expires_at",
                table: "group_invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_group_invitations_group_id_invitee_id_status",
                table: "group_invitations",
                columns: new[] { "group_id", "invitee_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_group_invitations_invitee_id",
                table: "group_invitations",
                column: "invitee_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_invitations_inviter_id",
                table: "group_invitations",
                column: "inviter_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_invitations_token",
                table: "group_invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_join_tokens_created_by_id",
                table: "group_join_tokens",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_join_tokens_expires_at",
                table: "group_join_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_group_join_tokens_group_id",
                table: "group_join_tokens",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_join_tokens_token",
                table: "group_join_tokens",
                column: "token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_contest_memberships_contest_attempts_active_attempt_id",
                table: "contest_memberships",
                column: "active_attempt_id",
                principalTable: "contest_attempts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_contest_memberships_group_invitations_invitation_id",
                table: "contest_memberships",
                column: "invitation_id",
                principalTable: "group_invitations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_group_memberships_group_invitations_invitation_id",
                table: "group_memberships",
                column: "invitation_id",
                principalTable: "group_invitations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_group_memberships_users_invited_by_id",
                table: "group_memberships",
                column: "invited_by_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_user_submits_contest_attempts_contest_attempt_id",
                table: "user_submits",
                column: "contest_attempt_id",
                principalTable: "contest_attempts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contest_memberships_contest_attempts_active_attempt_id",
                table: "contest_memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_contest_memberships_group_invitations_invitation_id",
                table: "contest_memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_group_memberships_group_invitations_invitation_id",
                table: "group_memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_group_memberships_users_invited_by_id",
                table: "group_memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_user_submits_contest_attempts_contest_attempt_id",
                table: "user_submits");

            migrationBuilder.DropTable(
                name: "contest_attempt_mission_results");

            migrationBuilder.DropTable(
                name: "group_invitations");

            migrationBuilder.DropTable(
                name: "group_join_tokens");

            migrationBuilder.DropTable(
                name: "contest_attempts");

            migrationBuilder.DropIndex(
                name: "ix_user_submits_contest_attempt_id",
                table: "user_submits");

            migrationBuilder.DropIndex(
                name: "ix_group_memberships_invitation_id",
                table: "group_memberships");

            migrationBuilder.DropIndex(
                name: "ix_group_memberships_invited_by_id",
                table: "group_memberships");

            migrationBuilder.DropIndex(
                name: "ix_contests_visibility",
                table: "contests");

            migrationBuilder.DropIndex(
                name: "ix_contest_memberships_active_attempt_id",
                table: "contest_memberships");

            migrationBuilder.DropIndex(
                name: "ix_contest_memberships_invitation_id",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "contest_attempt_id",
                table: "user_submits");

            migrationBuilder.DropColumn(
                name: "invitation_id",
                table: "group_memberships");

            migrationBuilder.DropColumn(
                name: "invited_by_id",
                table: "group_memberships");

            migrationBuilder.DropColumn(
                name: "is_auto_joined",
                table: "group_memberships");

            migrationBuilder.DropColumn(
                name: "joined_at",
                table: "group_memberships");

            migrationBuilder.DropColumn(
                name: "allow_early_finish",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "max_attempts",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "contests");

            migrationBuilder.DropColumn(
                name: "active_attempt_id",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "invitation_id",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "is_auto_joined",
                table: "contest_memberships");

            migrationBuilder.DropColumn(
                name: "joined_at",
                table: "contest_memberships");

            migrationBuilder.RenameColumn(
                name: "last_attempt_started_at",
                table: "contest_memberships",
                newName: "active_attempt_started_at");

            migrationBuilder.AddColumn<DateTime>(
                name: "active_attempt_expires_at",
                table: "contest_memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "attempt_count",
                table: "contest_memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
