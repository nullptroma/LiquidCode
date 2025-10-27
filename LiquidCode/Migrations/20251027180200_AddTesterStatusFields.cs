using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class AddTesterStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "amount_of_tests",
                table: "solutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "callback_token",
                table: "solutions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "current_test",
                table: "solutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "testing_error_code",
                table: "solutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "testing_message",
                table: "solutions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "testing_state",
                table: "solutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_of_tests",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "callback_token",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "current_test",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "testing_error_code",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "testing_message",
                table: "solutions");

            migrationBuilder.DropColumn(
                name: "testing_state",
                table: "solutions");
        }
    }
}
