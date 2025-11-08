using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCariStokTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OnayToken",
                table: "Teklifler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnayTokenGecerlilik",
                table: "Teklifler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnayZamani",
                table: "Teklifler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnaylayanAd",
                table: "Teklifler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedNotu",
                table: "Teklifler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RedZamani",
                table: "Teklifler",
                type: "datetime2",
                nullable: true);

            // Önce geçersiz CariId değerlerini geçerli bir CariId'ye ata (varsa)
            migrationBuilder.Sql(@"
                DECLARE @FirstValidCariId INT;
                SELECT TOP 1 @FirstValidCariId = Id FROM Cariler ORDER BY Id;
                
                IF @FirstValidCariId IS NOT NULL
                BEGIN
                    UPDATE Teklifler 
                    SET CariId = @FirstValidCariId 
                    WHERE CariId IS NOT NULL 
                    AND CariId NOT IN (SELECT Id FROM Cariler);
                END
                ELSE
                BEGIN
                    -- Eğer hiç Cari yoksa, geçersiz kayıtları sil
                    DELETE FROM Teklifler 
                    WHERE CariId IS NOT NULL 
                    AND CariId NOT IN (SELECT Id FROM Cariler);
                END
            ");

            // Index ve Foreign Key'leri SQL ile conditional olarak ekle
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teklifler_CariId' AND object_id = OBJECT_ID('Teklifler'))
                BEGIN
                    CREATE INDEX IX_Teklifler_CariId ON Teklifler(CariId);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Teklifler_Cariler_CariId')
                BEGIN
                    ALTER TABLE [Teklifler] 
                    ADD CONSTRAINT [FK_Teklifler_Cariler_CariId] 
                    FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([Id]) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teklifler_Cariler_CariId",
                table: "Teklifler");

            migrationBuilder.DropIndex(
                name: "IX_Teklifler_CariId",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "OnayToken",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "OnayTokenGecerlilik",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "OnayZamani",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "OnaylayanAd",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "RedNotu",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "RedZamani",
                table: "Teklifler");
        }
    }
}
