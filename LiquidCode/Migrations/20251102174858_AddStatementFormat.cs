using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class AddStatementFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "format",
                table: "mission_statements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_mission_statements_mission_id_language_format",
                table: "mission_statements",
                columns: new[] { "mission_id", "language", "format" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_mission_statements_mission_id_language_format",
                table: "mission_statements");

            migrationBuilder.DropColumn(
                name: "format",
                table: "mission_statements");
        }
    }
}
