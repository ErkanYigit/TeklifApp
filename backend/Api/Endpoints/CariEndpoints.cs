using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using Api.Contracts;

public static class CariEndpoints
{
    public static void MapCari(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/cari").RequireAuthorization();
        g.MapGet("", async ([AsParameters] CariQuery q, AppDbContext db) =>
        {
            var qry = db.Set<Cari>().AsNoTracking();
            if (!string.IsNullOrEmpty(q.Search))
                qry = qry.Where(x => x.Kod.Contains(q.Search) || x.Ad.Contains(q.Search) || x.VergiNo.Contains(q.Search));
            qry = (q.Sort ?? "Ad").ToLower() switch
            {
                "kod" => (q.Desc ? qry.OrderByDescending(x => x.Kod) : qry.OrderBy(x => x.Kod)),
                _ => (q.Desc ? qry.OrderByDescending(x => x.Ad) : qry.OrderBy(x => x.Ad))
            };
            var total = await qry.CountAsync();
            var items = await qry.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(x => new CariDto(x.Id, x.Kod, x.Ad, x.VergiNo, x.VergiDairesi, x.Telefon, x.Eposta))
            .ToListAsync();
            return Results.Ok(new Paged<CariDto>(items, total));
        });
        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var cari = await db.Set<Cari>().Include(x => x.Adresler).FirstOrDefaultAsync(x => x.Id == id);
            if (cari is null) return Results.NotFound();
            return Results.Ok(new {
                cari = new CariDto(cari.Id, cari.Kod, cari.Ad, cari.VergiNo, cari.VergiDairesi, cari.Telefon, cari.Eposta),
                adresler = cari.Adresler.Select(x => new AdresDto(x.Id, x.CariId, x.Tur, x.Ulke, x.Il, x.Ilce, x.Satir1, x.Satir2, x.PostaKodu)).ToList()
            });
        });
        g.MapPost("", async (CreateCariRequest req, AppDbContext db, HttpContext ctx) =>
        {
            if (string.IsNullOrWhiteSpace(req.Kod)) return Results.BadRequest(new { error = "Kod zorunlu" });
            if (string.IsNullOrWhiteSpace(req.Ad)) return Results.BadRequest(new { error = "Ad zorunlu" });
            if (string.IsNullOrWhiteSpace(req.VergiNo)) return Results.BadRequest(new { error = "Vergi no zorunlu" });
            if (string.IsNullOrWhiteSpace(req.VergiDairesi)) return Results.BadRequest(new { error = "Vergi dairesi zorunlu" });
            if (string.IsNullOrWhiteSpace(req.Telefon)) return Results.BadRequest(new { error = "Telefon zorunlu" });
            if (string.IsNullOrWhiteSpace(req.Eposta)) return Results.BadRequest(new { error = "E-posta zorunlu" });
            req = req with {
                Kod = req.Kod?.Trim() ?? string.Empty,
                Ad = req.Ad?.Trim() ?? string.Empty,
                VergiNo = req.VergiNo?.Trim() ?? string.Empty,
                VergiDairesi = req.VergiDairesi?.Trim() ?? string.Empty,
                Telefon = req.Telefon?.Trim() ?? string.Empty,
                Eposta = req.Eposta?.Trim() ?? string.Empty
            };
            if (!string.IsNullOrWhiteSpace(req.VergiNo) && !(req.VergiNo.All(char.IsDigit) && (req.VergiNo.Length == 10 || req.VergiNo.Length == 11)))
                return Results.BadRequest(new { error = "Vergi no 10-11 haneli numerik olmalı" });
            if (!string.IsNullOrWhiteSpace(req.Eposta) && !req.Eposta.Contains('@'))
                return Results.BadRequest(new { error = "E-posta formatı geçersiz" });
            if (await db.Set<Cari>().AnyAsync(x => x.Kod == req.Kod))
                return Results.Conflict(new { error = "Kod zaten kayıtlı" });
            var c = new Cari { Kod = req.Kod, Ad = req.Ad, VergiNo = req.VergiNo, VergiDairesi = req.VergiDairesi, Telefon = req.Telefon, Eposta = req.Eposta, CreatedByUserId = GetUserId(ctx) };
            db.Add(c); await db.SaveChangesAsync();
            return Results.Created($"/cari/{c.Id}", new { c.Id});
        });
        g.MapPut("/{id:int}", async (int id, UpdateCariRequest req, AppDbContext db, HttpContext ctx) =>
        {
            var c = await db.Set<Cari>().FindAsync(id); if (c is null) return Results.NotFound();
            var uid = GetUserId(ctx);
            var isAdmin = IsAdmin(ctx);
            // Admin değilse, yalnızca kendi oluşturduğu kaydı güncelleyebilir. Sahibi yoksa (null) da izin verme.
            if (!isAdmin && (c.CreatedByUserId.GetValueOrDefault() != uid))
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            if (string.IsNullOrWhiteSpace(req.Ad)) return Results.BadRequest(new { error = "Ad zorunlu" });
            if (string.IsNullOrWhiteSpace(req.VergiNo)) return Results.BadRequest(new { error = "Vergi no zorunlu" });
            if (string.IsNullOrWhiteSpace(req.VergiDairesi)) return Results.BadRequest(new { error = "Vergi dairesi zorunlu" });
            if (string.IsNullOrWhiteSpace(req.Telefon)) return Results.BadRequest(new { error = "Telefon zorunlu" });
            if (string.IsNullOrWhiteSpace(req.Eposta)) return Results.BadRequest(new { error = "E-posta zorunlu" });
            req = req with {
                Ad = req.Ad?.Trim() ?? string.Empty,
                VergiNo = req.VergiNo?.Trim() ?? string.Empty,
                VergiDairesi = req.VergiDairesi?.Trim() ?? string.Empty,
                Telefon = req.Telefon?.Trim() ?? string.Empty,
                Eposta = req.Eposta?.Trim() ?? string.Empty
            };
            if (!string.IsNullOrWhiteSpace(req.VergiNo) && !(req.VergiNo.All(char.IsDigit) && (req.VergiNo.Length == 10 || req.VergiNo.Length == 11)))
                return Results.BadRequest(new { error = "Vergi no 10-11 haneli numerik olmalı" });
            if (!string.IsNullOrWhiteSpace(req.Eposta) && !req.Eposta.Contains('@'))
                return Results.BadRequest(new { error = "E-posta formatı geçersiz" });
            c.Ad = req.Ad; c.VergiNo = req.VergiNo; c.VergiDairesi = req.VergiDairesi; c.Telefon = req.Telefon; c.Eposta = req.Eposta;
            await db.SaveChangesAsync(); 
            return Results.NoContent();
        });
        g.MapDelete("/{id:int}", async (int id, AppDbContext db, HttpContext ctx) =>
        {
            var c = await db.Set<Cari>().FindAsync(id); if (c is null) return Results.NotFound();
            var uid = GetUserId(ctx);
            var isAdmin = IsAdmin(ctx);
            // Admin değilse, yalnızca kendi oluşturduğu kaydı silebilir. Sahibi yoksa (null) da izin verme.
            if (!isAdmin && (c.CreatedByUserId.GetValueOrDefault() != uid))
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            var hasTeklif = await db.Set<Teklif>().AnyAsync(t => t.CariId == id);
            if (hasTeklif) return Results.Conflict(new { error = "Cari bağlı teklifler olduğu için silinemez" });
            db.Remove(c); await db.SaveChangesAsync();
            return Results.NoContent();
        });
        // adresler (alias: /adresler)
        g.MapGet("/{id:int}/adres", async (int id, AppDbContext db) =>
        {
            if (!await db.Set<Cari>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var items = await db.Set<CariAdres>().Where(x => x.CariId == id)
            .Select(x => new AdresDto(x.Id, x.CariId, x.Tur, x.Ulke, x.Il, x.Ilce, x.Satir1, x.Satir2, x.PostaKodu)).ToListAsync();
            return Results.Ok(items);
        });
        g.MapGet("/{id:int}/adresler", async (int id, AppDbContext db) =>
        {
            if (!await db.Set<Cari>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var items = await db.Set<CariAdres>().Where(x => x.CariId == id)
            .Select(x => new AdresDto(x.Id, x.CariId, x.Tur, x.Ulke, x.Il, x.Ilce, x.Satir1, x.Satir2, x.PostaKodu)).ToListAsync();
            return Results.Ok(items);
        });
        g.MapPost("/{id:int}/adres", async (int id, CreateAdresRequest req, AppDbContext db) =>
        {
            if (!await db.Set<Cari>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var a = new CariAdres { CariId = id, Tur = req.Tur, Ulke = req.Ulke, Il = req.Il, Ilce = req.Ilce, Satir1 = req.Satir1, Satir2 = req.Satir2, PostaKodu = req.PostaKodu };
            db.Add(a); await db.SaveChangesAsync();
            return Results.Created($"/cari/{id}/adres/{a.Id}", new { a.Id});
        });
        g.MapPost("/{id:int}/adresler", async (int id, CreateAdresRequest req, AppDbContext db) =>
        {
            if (!await db.Set<Cari>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var a = new CariAdres { CariId = id, Tur = req.Tur, Ulke = req.Ulke, Il = req.Il, Ilce = req.Ilce, Satir1 = req.Satir1, Satir2 = req.Satir2, PostaKodu = req.PostaKodu };
            db.Add(a); await db.SaveChangesAsync();
            return Results.Created($"/cari/{id}/adres/{a.Id}", new { a.Id});
        });
        g.MapPut("/{id:int}/adres/{adresId:int}", async (int id, int adresId, UpdateAdresRequest req, AppDbContext db) =>
        {
            var a = await db.Set<CariAdres>().FirstOrDefaultAsync(x => x.Id == adresId && x.CariId == id);
            if (a is null) return Results.NotFound();
            a.Tur = req.Tur; a.Ulke = req.Ulke; a.Il = req.Il; a.Ilce = req.Ilce; a.Satir1 = req.Satir1; a.Satir2 = req.Satir2; a.PostaKodu = req.PostaKodu;
            await db.SaveChangesAsync(); return Results.NoContent();
        });
        g.MapPut("/{id:int}/adresler/{adresId:int}", async (int id, int adresId, UpdateAdresRequest req, AppDbContext db) =>
        {
            var a = await db.Set<CariAdres>().FirstOrDefaultAsync(x => x.Id == adresId && x.CariId == id);
            if (a is null) return Results.NotFound();
            a.Tur = req.Tur; a.Ulke = req.Ulke; a.Il = req.Il; a.Ilce = req.Ilce; a.Satir1 = req.Satir1; a.Satir2 = req.Satir2; a.PostaKodu = req.PostaKodu;
            await db.SaveChangesAsync(); return Results.NoContent();
        });
        g.MapDelete("/{id:int}/adres/{adresId:int}", async (int id, int adresId, AppDbContext db) =>
        {
            var a = await db.Set<CariAdres>().FirstOrDefaultAsync(x => x.Id == adresId && x.CariId == id);
            if (a is null) return Results.NotFound();
            db.Remove(a); await db.SaveChangesAsync();
            return Results.NoContent();
        });
        g.MapDelete("/{id:int}/adresler/{adresId:int}", async (int id, int adresId, AppDbContext db) =>
        {
            var a = await db.Set<CariAdres>().FirstOrDefaultAsync(x => x.Id == adresId && x.CariId == id);
            if (a is null) return Results.NotFound();
            db.Remove(a); await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static int? GetUserId(HttpContext ctx)
    {
        var sub = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (int.TryParse(sub, out var id)) return id;
        return null;
    }

    private static bool IsAdmin(HttpContext ctx) => ctx.User?.IsInRole("Admin") == true;
}