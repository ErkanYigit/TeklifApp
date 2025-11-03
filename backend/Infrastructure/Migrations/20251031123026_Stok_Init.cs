using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Stok_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stoklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cinsi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stoklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StokFiyatlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    FiyatListeNo = table.Column<int>(type: "int", nullable: false),
                    Deger = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YururlukTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokFiyatlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokFiyatlar_Stoklar_StokId",
                        column: x => x.StokId,
                        principalTable: "Stoklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StokFiyatlar_StokId_FiyatListeNo_YururlukTarihi",
                table: "StokFiyatlar",
                columns: new[] { "StokId", "FiyatListeNo", "YururlukTarihi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stoklar_Kod",
                table: "Stoklar",
                column: "Kod",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StokFiyatlar");

            migrationBuilder.DropTable(
                name: "Stoklar");
        }
    }
}
