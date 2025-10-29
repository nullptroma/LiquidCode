using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    pass_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    salt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    schedule_type = table.Column<int>(type: "integer", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempt_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    group_id = table.Column<int>(type: "integer", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contests", x => x.id);
                    table.ForeignKey(
                        name: "fk_contests_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "articles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    s3key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_articles_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_memberships",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_memberships", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_group_memberships_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_memberships_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "missions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    s3key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_missions", x => x.id);
                    table.ForeignKey(
                        name: "fk_missions_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    db_user_id = table.Column<int>(type: "integer", nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    os_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.token);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_db_user_id",
                        column: x => x.db_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contest_memberships",
                columns: table => new
                {
                    contest_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    active_attempt_started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    active_attempt_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_memberships", x => new { x.contest_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_contest_memberships_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "contests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_memberships_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_tags",
                columns: table => new
                {
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    tag_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_tags", x => new { x.article_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_article_tags_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_article_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contest_articles",
                columns: table => new
                {
                    contest_id = table.Column<int>(type: "integer", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_articles", x => new { x.contest_id, x.article_id });
                    table.ForeignKey(
                        name: "fk_contest_articles_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_articles_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "contests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contest_missions",
                columns: table => new
                {
                    contest_id = table.Column<int>(type: "integer", nullable: false),
                    mission_id = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_missions", x => new { x.contest_id, x.mission_id });
                    table.ForeignKey(
                        name: "fk_contest_missions_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "contests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contest_missions_missions_mission_id",
                        column: x => x.mission_id,
                        principalTable: "missions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mission_statements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mission_id = table.Column<int>(type: "integer", nullable: false),
                    language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatementTextsJson = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mission_statements", x => x.id);
                    table.ForeignKey(
                        name: "fk_mission_statements_missions_mission_id",
                        column: x => x.mission_id,
                        principalTable: "missions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mission_tags",
                columns: table => new
                {
                    mission_id = table.Column<int>(type: "integer", nullable: false),
                    tag_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mission_tags", x => new { x.mission_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_mission_tags_missions_mission_id",
                        column: x => x.mission_id,
                        principalTable: "missions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mission_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solutions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mission_id = table.Column<int>(type: "integer", nullable: false),
                    language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    language_version = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    source_code = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    testing_state = table.Column<int>(type: "integer", nullable: false),
                    testing_error_code = table.Column<int>(type: "integer", nullable: false),
                    testing_message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    current_test = table.Column<int>(type: "integer", nullable: false),
                    amount_of_tests = table.Column<int>(type: "integer", nullable: false),
                    callback_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solutions", x => x.id);
                    table.ForeignKey(
                        name: "fk_solutions_missions_mission_id",
                        column: x => x.mission_id,
                        principalTable: "missions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mission_statement_medias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    statement_id = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    media_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    media_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mission_statement_medias", x => x.id);
                    table.ForeignKey(
                        name: "fk_mission_statement_medias_mission_statements_statement_id",
                        column: x => x.statement_id,
                        principalTable: "mission_statements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_submits",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    solution_id = table.Column<int>(type: "integer", nullable: false),
                    contest_id = table.Column<int>(type: "integer", nullable: true),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_submits", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_submits_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "contests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_submits_solutions_solution_id",
                        column: x => x.solution_id,
                        principalTable: "solutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_submits_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_tags_tag_id",
                table: "article_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_articles_author_id",
                table: "articles",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_articles_is_deleted",
                table: "articles",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_contest_articles_article_id",
                table: "contest_articles",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_contest_memberships_role",
                table: "contest_memberships",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_contest_memberships_user_id",
                table: "contest_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_contest_missions_mission_id",
                table: "contest_missions",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_contests_available_from",
                table: "contests",
                column: "available_from");

            migrationBuilder.CreateIndex(
                name: "ix_contests_available_until",
                table: "contests",
                column: "available_until");

            migrationBuilder.CreateIndex(
                name: "ix_contests_ends_at",
                table: "contests",
                column: "ends_at");

            migrationBuilder.CreateIndex(
                name: "ix_contests_group_id",
                table: "contests",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_contests_is_deleted",
                table: "contests",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_contests_starts_at",
                table: "contests",
                column: "starts_at");

            migrationBuilder.CreateIndex(
                name: "ix_group_memberships_role",
                table: "group_memberships",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_group_memberships_user_id",
                table: "group_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_is_deleted",
                table: "groups",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_mission_statement_medias_media_key",
                table: "mission_statement_medias",
                column: "media_key");

            migrationBuilder.CreateIndex(
                name: "ix_mission_statement_medias_statement_id",
                table: "mission_statement_medias",
                column: "statement_id");

            migrationBuilder.CreateIndex(
                name: "ix_mission_statements_language",
                table: "mission_statements",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "ix_mission_statements_mission_id",
                table: "mission_statements",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_mission_tags_tag_id",
                table: "mission_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_missions_author_id",
                table: "missions",
                column: "author_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_db_user_id",
                table: "refresh_tokens",
                column: "db_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires",
                table: "refresh_tokens",
                column: "expires");

            migrationBuilder.CreateIndex(
                name: "ix_solutions_created_at",
                table: "solutions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_solutions_mission_id",
                table: "solutions",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_solutions_status",
                table: "solutions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tags_is_deleted",
                table: "tags",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_contest_id",
                table: "user_submits",
                column: "contest_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_created_at",
                table: "user_submits",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_is_deleted",
                table: "user_submits",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_solution_id",
                table: "user_submits",
                column: "solution_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_submits_user_id",
                table: "user_submits",
                column: "user_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_tags");

            migrationBuilder.DropTable(
                name: "contest_articles");

            migrationBuilder.DropTable(
                name: "contest_memberships");

            migrationBuilder.DropTable(
                name: "contest_missions");

            migrationBuilder.DropTable(
                name: "group_memberships");

            migrationBuilder.DropTable(
                name: "mission_statement_medias");

            migrationBuilder.DropTable(
                name: "mission_tags");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_submits");

            migrationBuilder.DropTable(
                name: "articles");

            migrationBuilder.DropTable(
                name: "mission_statements");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "contests");

            migrationBuilder.DropTable(
                name: "solutions");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "missions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
