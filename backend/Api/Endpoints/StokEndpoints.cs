using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using Api.Contracts;

public static class StokEndpoints
{
    public static void MapStok(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/stok").RequireAuthorization();

        g.MapGet("", async ([AsParameters] StokQuery q, AppDbContext db) =>
        {
            var qry = db.Set<Stok>().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q.Search))
                qry = qry.Where(x => x.Kod.Contains(q.Search) || x.Ad.Contains(q.Search));
            if (q.Aktif.HasValue) qry = qry.Where(x => x.Aktif == q.Aktif);
            qry = (q.Sort ?? "Ad").ToLower() switch
            {
                "kod" => (q.Desc ? qry.OrderByDescending(x => x.Kod) : qry.OrderBy(x => x.Kod)),
                _ => (q.Desc ? qry.OrderByDescending(x => x.Ad) : qry.OrderBy(x => x.Ad))
            };
            var total = await qry.CountAsync();
            var items = await qry.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(x => new StokDto(x.Id, x.Kod, x.Ad, x.Birim, x.Cinsi, x.Aktif)).ToListAsync();
            return Results.Ok(new Paged<StokDto>(items, total));
        });
        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var stok = await db.Set<Stok>().Include(x => x.Fiyatlar).FirstOrDefaultAsync(x => x.Id == id);
            if (stok is null) return Results.NotFound();
            return Results.Ok(new {
                stok = new StokDto(stok.Id, stok.Kod, stok.Ad, stok.Birim, stok.Cinsi, stok.Aktif),
                fiyatlar = stok.Fiyatlar.Select(x => new StokFiyatDto(x.Id, x.StokId, x.FiyatListeNo, x.Deger, x.ParaBirimi, x.YururlukTarihi)).OrderByDescending(x => x.YururlukTarihi)
            });
        });
        g.MapPost("", async (CreateStokRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Kod)) return Results.BadRequest(new { error = "Kod zorunlu" });
            if (string.IsNullOrWhiteSpace(req.Ad)) return Results.BadRequest(new { error = "Ad zorunlu" });
            req = req with { Kod = req.Kod?.Trim() ?? "", Ad = req.Ad?.Trim() ?? "", Birim = req.Birim?.Trim(), Cins = req.Cins?.Trim() };
            if (await db.Set<Stok>().AnyAsync(x => x.Kod == req.Kod))
                return Results.Conflict(new { error = "Kod zaten kayıtlı" });
            var stok = new Stok { Kod = req.Kod, Ad = req.Ad, Birim = req.Birim, Cinsi = req.Cins, Aktif = req.Aktif };
            db.Add(stok); await db.SaveChangesAsync();
            return Results.Created($"/stok/{stok.Id}", new { stok.Id});
        }).RequireAuthorization("AdminOrPurchase");
        g.MapPut("/{id:int}", async (int id, UpdateStokRequest req, AppDbContext db) =>
        {
            var stok = await db.Set<Stok>().FindAsync(id); if (stok is null) return Results.NotFound();
            if (req.Ad is not null) {
                if (string.IsNullOrWhiteSpace(req.Ad)) return Results.BadRequest(new { error = "Ad zorunlu" });
                stok.Ad = req.Ad.Trim();
            }
            if (req.Birim is not null) stok.Birim = req.Birim;
            if (req.Cins is not null) stok.Cinsi = req.Cins;
            if (req.Aktif.HasValue) stok.Aktif = req.Aktif.Value;
            await db.SaveChangesAsync(); return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");
        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var stok = await db.Set<Stok>().FindAsync(id); if (stok is null) return Results.NotFound();
            var linked = await db.Set<TeklifKalem>().AnyAsync(k => k.StokId == id) || await db.Set<TeklifSepetKalem>().AnyAsync(k => k.StokId == id);
            if (linked) return Results.Conflict(new { error = "Stok bağlı kayıtlar olduğu için silinemez" });
            db.Remove(stok); await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");
        g.MapGet("/{id:int}/fiyatlar", async (int id, AppDbContext db) =>
        {
            if (!await db.Set<Stok>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var items = await db.Set<StokFiyat>().Where(x => x.StokId == id).OrderByDescending(x => x.YururlukTarihi)
            .Select(x => new StokFiyatDto(x.Id, x.StokId, x.FiyatListeNo, x.Deger, x.ParaBirimi, x.YururlukTarihi)).ToListAsync();
            return Results.Ok(items);
        });
        // Aktif fiyat hesapla (UDF yerine servis)
        g.MapGet("/{id:int}/fiyat", async (int id, int? liste, DateTime? tarih, AppDbContext db) =>
        {
            if (!await db.Set<Stok>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var f = await Infrastructure.Services.FiyatServisi.GetAktifFiyatAsync(db, id, liste, tarih);
            if (f is null) return Results.NotFound(new { error = "Fiyat bulunamadı" });
            return Results.Ok(new { fiyat = f });
        });
        g.MapPost("/{id:int}/fiyatlar", async (int id, CreateStokFiyatRequest req, AppDbContext db) =>
        {
            if (!await db.Set<Stok>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var fiyat = new StokFiyat { StokId = id, FiyatListeNo = req.FiyatListeNo, Deger = req.Deger, ParaBirimi = req.ParaBirimi, YururlukTarihi = req.YururlukTarihi };
            db.Add(fiyat); await db.SaveChangesAsync();
            return Results.Created($"/stok/{id}/fiyatlar/{fiyat.Id}", new { fiyat.Id});
        }).RequireAuthorization("AdminOrPurchase");
        g.MapPut("/{id:int}/fiyatlar/{fid:int}", async (int id, int fid, UpdateStokFiyatRequest req, AppDbContext db) =>
        {
            var fiyat = await db.Set<StokFiyat>().FirstOrDefaultAsync(x => x.Id == fid && x.StokId == id);
            if (fiyat is null) return Results.NotFound();
            fiyat.FiyatListeNo = req.FiyatListeNo; fiyat.Deger = req.Deger; fiyat.ParaBirimi = req.ParaBirimi; fiyat.YururlukTarihi = req.YururlukTarihi;
            await db.SaveChangesAsync(); 
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");
        g.MapDelete("/{id:int}/fiyatlar/{fid:int}", async (int id, int fid, AppDbContext db) =>
        {
            var fiyat = await db.Set<StokFiyat>().FirstOrDefaultAsync(x => x.Id == fid && x.StokId == id);
            if (fiyat is null) return Results.NotFound();
            db.Remove(fiyat); await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");

        // TedarikciFiyat endpoints
        g.MapGet("/{id:int}/tedarikci-fiyatlar", async (int id, AppDbContext db) =>
        {
            if (!await db.Set<Stok>().AnyAsync(x => x.Id == id)) return Results.NotFound();
            var items = await (from tf in db.Set<TedarikciFiyat>()
                             join t in db.Set<Tedarikci>() on tf.TedarikciId equals t.Id
                             where tf.StokId == id
                             orderby tf.GuncellemeTarihi descending
                             select new TedarikciFiyatDto(tf.Id, tf.StokId, tf.TedarikciId, t.Ad, tf.FiyatListeNo, tf.Fiyat, tf.ParaBirimi, tf.GuncellemeTarihi)).ToListAsync();
            return Results.Ok(items);
        });

        g.MapPost("/{id:int}/tedarikci-fiyatlar", async (int id, CreateTedarikciFiyatRequest req, AppDbContext db) =>
        {
            if (!await db.Set<Stok>().AnyAsync(x => x.Id == id)) return Results.NotFound(new { error = "Stok bulunamadı" });
            if (!await db.Set<Tedarikci>().AnyAsync(x => x.Id == req.TedarikciId)) return Results.NotFound(new { error = "Tedarikçi bulunamadı" });
            var fiyat = new TedarikciFiyat
            {
                StokId = id,
                TedarikciId = req.TedarikciId,
                FiyatListeNo = req.FiyatListeNo,
                Fiyat = req.Fiyat,
                ParaBirimi = req.ParaBirimi ?? "TRY",
                GuncellemeTarihi = req.GuncellemeTarihi ?? DateTime.UtcNow
            };
            db.Add(fiyat);
            await db.SaveChangesAsync();
            return Results.Created($"/stok/{id}/tedarikci-fiyatlar/{fiyat.Id}", new { fiyat.Id });
        }).RequireAuthorization("AdminOrPurchase");

        g.MapPut("/{id:int}/tedarikci-fiyatlar/{fid:int}", async (int id, int fid, UpdateTedarikciFiyatRequest req, AppDbContext db) =>
        {
            var fiyat = await db.Set<TedarikciFiyat>().FirstOrDefaultAsync(x => x.Id == fid && x.StokId == id);
            if (fiyat is null) return Results.NotFound();
            if (!await db.Set<Tedarikci>().AnyAsync(x => x.Id == req.TedarikciId)) return Results.NotFound(new { error = "Tedarikçi bulunamadı" });
            fiyat.TedarikciId = req.TedarikciId;
            fiyat.FiyatListeNo = req.FiyatListeNo;
            fiyat.Fiyat = req.Fiyat;
            fiyat.ParaBirimi = req.ParaBirimi ?? "TRY";
            fiyat.GuncellemeTarihi = req.GuncellemeTarihi ?? DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");

        g.MapDelete("/{id:int}/tedarikci-fiyatlar/{fid:int}", async (int id, int fid, AppDbContext db) =>
        {
            var fiyat = await db.Set<TedarikciFiyat>().FirstOrDefaultAsync(x => x.Id == fid && x.StokId == id);
            if (fiyat is null) return Results.NotFound();
            db.Remove(fiyat);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");
    }
}
    