using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Gun6_Sepet_Foy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeklifSepetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifSepetler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeklifSepetKalemler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SepetId = table.Column<int>(type: "int", nullable: false),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HedefFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifSepetKalemler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifSepetKalemler_TeklifSepetler_SepetId",
                        column: x => x.SepetId,
                        principalTable: "TeklifSepetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeklifSepetKalemler_SepetId",
                table: "TeklifSepetKalemler",
                column: "SepetId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifSepetler_UserId",
                table: "TeklifSepetler",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeklifSepetKalemler");

            migrationBuilder.DropTable(
                name: "TeklifSepetler");
        }
    }
}
