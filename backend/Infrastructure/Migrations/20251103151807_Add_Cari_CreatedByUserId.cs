using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Cari_CreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Cariler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_CreatedByUserId",
                table: "Cariler",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cariler_CreatedByUserId",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Cariler");
        }
    }
}
