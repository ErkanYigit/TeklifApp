using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Data;
using Api.Contracts;
using System.Text;

public static class FoyEndpoints
{
    public static void MapFoy(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/teklif/foy").RequireAuthorization();

        // Listeleme
        g.MapGet("", async ([AsParameters] FoyQuery q, AppDbContext db) =>
        {
            var qry = db.Set<Teklif>().AsNoTracking();
            if (q.Baslangic.HasValue)
                qry = qry.Where(x => x.TeklfiTarihi >= q.Baslangic);
            if (q.Bitis.HasValue)
                qry = qry.Where(x => x.TeklfiTarihi < q.Bitis);
            if (q.CariId.HasValue)
                qry = qry.Where(x => x.CariId == q.CariId);
            if (!string.IsNullOrWhiteSpace(q.Durum))
                qry = qry.Where(x => x.Durum == q.Durum);
            if (!string.IsNullOrWhiteSpace(q.Ara))
                qry = qry.Where(x => x.No.Contains(q.Ara));
            var total = await qry.CountAsync();
            var items = await qry.OrderByDescending(x => x.TeklfiTarihi)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(t => new FoyItem(t.Id, t.No, t.CariId, t.TeklfiTarihi, t.Durum, t.GenelToplam))
                .ToListAsync();
            return Results.Ok(new Paged<FoyItem>(items, total));
        });

        // Benim Föyüm
        g.MapGet("/my", async ([AsParameters] FoyQuery q, AppDbContext db, HttpContext ctx) =>
        {
            var uidClaim = ctx.User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            int? uid = int.TryParse(uidClaim, out var idVal) ? idVal : null;
            if (uid is null) return Results.Unauthorized();
            var qry = db.Set<Teklif>().AsNoTracking().Where(t => t.CreatedByUserId == uid);
            if (q.Baslangic.HasValue)
                qry = qry.Where(x => x.TeklfiTarihi >= q.Baslangic);
            if (q.Bitis.HasValue)
                qry = qry.Where(x => x.TeklfiTarihi < q.Bitis);
            if (q.CariId.HasValue)
                qry = qry.Where(x => x.CariId == q.CariId);
            if (!string.IsNullOrWhiteSpace(q.Durum))
                qry = qry.Where(x => x.Durum == q.Durum);
            if (!string.IsNullOrWhiteSpace(q.Ara))
                qry = qry.Where(x => x.No.Contains(q.Ara));
            var total = await qry.CountAsync();
            var items = await qry.OrderByDescending(x => x.TeklfiTarihi)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(t => new FoyItem(t.Id, t.No, t.CariId, t.TeklfiTarihi, t.Durum, t.GenelToplam))
                .ToListAsync();
            return Results.Ok(new Paged<FoyItem>(items, total));
        });

        // CSV export (tüm eşleşenler)
        g.MapGet("/export/csv", async ([AsParameters] FoyQuery q, AppDbContext db) =>
        {
            var rows = await Filter(db.Set<Teklif>().AsNoTracking(), q)
                .OrderByDescending(x => x.TeklfiTarihi)
                .Select(t => new { t.No, t.CariId, t.TeklfiTarihi, t.Durum, t.GenelToplam })
                .ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("No,CariId,Tarih,Durum,GenelToplam");
            foreach (var r in rows)
                sb.AppendLine($"{r.No},{r.CariId},{r.TeklfiTarihi:yyyy-MM-dd},{r.Durum},{r.GenelToplam}");
            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "teklif_foy.csv");
        });

        // Basit Excel (OpenXML veya CSV-xlsx); burada CSV'yi xlsx content-type ile verme seçeneği basitleştirildi
        g.MapGet("/export/xlsx", async ([AsParameters] FoyQuery q, AppDbContext db) =>
        {
            var rows = await Filter(db.Set<Teklif>().AsNoTracking(), q)
                .OrderByDescending(x => x.TeklfiTarihi)
                .Select(t => new { t.No, t.CariId, t.TeklfiTarihi, t.Durum, t.GenelToplam })
                .ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("No\tCariId\tTarih\tDurum\tGenelToplam");
            foreach (var r in rows)
                sb.AppendLine($"{r.No}\t{r.CariId}\t{r.TeklfiTarihi:yyyy-MM-dd}\t{r.Durum}\t{r.GenelToplam}");
            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", "teklif_foy.xls");
        });

        static IQueryable<Teklif> Filter(IQueryable<Teklif> qry, FoyQuery q)
        {
            if (q.Baslangic.HasValue)
                qry = qry.Where(x => x.TeklfiTarihi >= q.Baslangic);
            if (q.Bitis.HasValue)
                qry = qry.Where(x => x.TeklfiTarihi < q.Bitis);
            if (q.CariId.HasValue)
                qry = qry.Where(x => x.CariId == q.CariId);
            if (!string.IsNullOrWhiteSpace(q.Durum))
                qry = qry.Where(x => x.Durum == q.Durum);
            if (!string.IsNullOrWhiteSpace(q.Ara))
                qry = qry.Where(x => x.No.Contains(q.Ara));
            return qry;
        }
    }
}