using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Teklif_CreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Teklifler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_CreatedByUserId",
                table: "Teklifler",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teklifler_CreatedByUserId",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Teklifler");
        }
    }
}
