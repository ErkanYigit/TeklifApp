using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UDF_StokAktifFiyat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('dbo.fn_GetAktifFiyat','IF') IS NOT NULL DROP FUNCTION dbo.fn_GetAktifFiyat;");
            migrationBuilder.Sql(@"CREATE FUNCTION dbo.fn_GetAktifFiyat(@StokId INT, @FiyatListeNo INT = NULL, @Tarih DATETIME2 = NULL)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @ret DECIMAL(18,2);
    DECLARE @t DATETIME2 = ISNULL(@Tarih, SYSUTCDATETIME());
    SELECT TOP(1) @ret = Deger
    FROM StokFiyatlar WITH (NOLOCK)
    WHERE StokId = @StokId
      AND (@FiyatListeNo IS NULL OR FiyatListeNo = @FiyatListeNo)
      AND YururlukTarihi <= @t
    ORDER BY YururlukTarihi DESC;
    RETURN @ret;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('dbo.fn_GetAktifFiyat','IF') IS NOT NULL DROP FUNCTION dbo.fn_GetAktifFiyat;");
        }
    }
}
