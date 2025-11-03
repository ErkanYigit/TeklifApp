using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Karsilastirma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tedarikci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Eposta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tedarikci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TedarikciFiyat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    TedarikciId = table.Column<int>(type: "int", nullable: false),
                    FiyatListeNo = table.Column<int>(type: "int", nullable: false),
                    Fiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TedarikciFiyat", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tedarikci_Ad",
                table: "Tedarikci",
                column: "Ad");

            migrationBuilder.CreateIndex(
                name: "IX_TedarikciFiyat_StokId_FiyatListeNo_GuncellemeTarihi",
                table: "TedarikciFiyat",
                columns: new[] { "StokId", "FiyatListeNo", "GuncellemeTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_TedarikciFiyat_StokId_TedarikciId_FiyatListeNo",
                table: "TedarikciFiyat",
                columns: new[] { "StokId", "TedarikciId", "FiyatListeNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tedarikci");

            migrationBuilder.DropTable(
                name: "TedarikciFiyat");
        }
    }
}
