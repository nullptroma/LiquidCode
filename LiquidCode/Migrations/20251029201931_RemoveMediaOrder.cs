using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMediaOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order_index",
                table: "mission_statement_medias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "mission_statement_medias",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
