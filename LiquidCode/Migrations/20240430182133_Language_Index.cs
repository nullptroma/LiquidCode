using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidCode.Migrations
{
    /// <inheritdoc />
    public partial class Language_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_missions_text_data_mission_id",
                table: "missions_text_data");

            migrationBuilder.CreateIndex(
                name: "ix_missions_text_data_mission_id_language",
                table: "missions_text_data",
                columns: new[] { "mission_id", "language" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_missions_text_data_mission_id_language",
                table: "missions_text_data");

            migrationBuilder.CreateIndex(
                name: "ix_missions_text_data_mission_id",
                table: "missions_text_data",
                column: "mission_id");
        }
    }
}
