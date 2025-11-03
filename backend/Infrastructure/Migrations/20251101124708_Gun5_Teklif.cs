using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Gun5_Teklif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teklifler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    No = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CariId = table.Column<int>(type: "int", nullable: false),
                    TeklfiTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AraToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IskontoToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GenelToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teklifler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeklifKalemler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeklifId = table.Column<int>(type: "int", nullable: false),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IskontoOran = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvOran = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IskontoTutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    KdvTutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GenelTutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifKalemler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifKalemler_Teklifler_TeklifId",
                        column: x => x.TeklifId,
                        principalTable: "Teklifler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalemler_TeklifId",
                table: "TeklifKalemler",
                column: "TeklifId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_No",
                table: "Teklifler",
                column: "No",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeklifKalemler");

            migrationBuilder.DropTable(
                name: "Teklifler");
        }
    }
}
