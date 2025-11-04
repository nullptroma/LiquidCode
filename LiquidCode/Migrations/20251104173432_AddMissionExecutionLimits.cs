using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionExecutionLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "memory_limit_bytes",
                table: "missions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "time_limit_milliseconds",
                table: "missions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "memory_limit_bytes",
                table: "missions");

            migrationBuilder.DropColumn(
                name: "time_limit_milliseconds",
                table: "missions");
        }
    }
}
