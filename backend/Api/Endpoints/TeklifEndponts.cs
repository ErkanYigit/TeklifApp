using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Entities;
using Api.Contracts;
using Application.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public static class TeklifEndpoints
{
    public static void MapTeklif(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/teklif").RequireAuthorization();
        g.MapGet("", async (int page, int pageSize, int? cariId, string? durum, AppDbContext db) =>
        {
            var qry = db.Set<Teklif>().AsNoTracking();
            if (cariId.HasValue) qry = qry.Where(x => x.CariId == cariId);
            if (!string.IsNullOrWhiteSpace(durum)) qry = qry.Where(x => x.Durum == durum);
            var total = await qry.CountAsync();
            var items = await qry.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new TeklifDto(x.Id, x.No, x.CariId, x.TeklfiTarihi, x.Durum, x.AraToplam, x.IskontoToplam, x.KdvToplam, x.GenelToplam))
            .ToListAsync();
            return Results.Ok(new Paged<TeklifDto>(items, total));
        });
        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();
            var dto = new TeklifDto(t.Id, t.No, t.CariId, t.TeklfiTarihi, t.Durum, t.AraToplam, t.IskontoToplam, t.KdvToplam, t.GenelToplam);
            var kalemler = t.Kalemler.Select(x => new KalemDto(x.Id, x.TeklifId, x.StokId, x.Miktar, x.BirimFiyat, x.IskontoOran, x.KdvOran, x.Tutar, x.IskontoTutar, x.KdvTutar, x.GenelTutar)).ToList();
            return Results.Ok(new { teklif = dto, kalemler = kalemler });
        });
        g.MapPost("", async (CreateTeklifRequest req, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var uid = GetUserId(ctx);
            var t = new Teklif { CariId = req.CariId, No = NoUretici.TeklifNo(db), CreatedByUserId = uid };

            if (req.Kalemler is not null && req.Kalemler.Count > 0)
            {
                foreach (var k in req.Kalemler)
                {
                    t.Kalemler.Add(new TeklifKalem
                    {
                        StokId = k.StokId,
                        Miktar = k.Miktar,
                        BirimFiyat = k.BirimFiyat,
                        IskontoOran = k.IskontoOran,
                        KdvOran = k.KdvOran
                    });
                }
                TeklifHesap.Hesapla(t);
            }

            db.Set<Teklif>().Add(t); 
            await db.SaveChangesAsync();
            Audit.Log(db, nameof(Teklif), t.Id, "Oluşturuldu", null, new { t.CariId, KalemSayisi = t.Kalemler.Count, t.GenelToplam }, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.Created($"/teklif/{t.Id}", new { t.Id, t.No });
        });
        g.MapPut("/{id:int}", async (int id, UpdateTeklifRequest req, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id); 
            if (t is null) return Results.NotFound();
            if (!IsEditable(t)) return Results.Conflict(new { error = "Teklif durumu nedeniyle düzenlenemez" });
            var uid = GetUserId(ctx); var isAdmin = IsAdmin(ctx);
            if (!isAdmin && t.CreatedByUserId.HasValue && t.CreatedByUserId != uid) return Results.StatusCode(StatusCodes.Status403Forbidden);
            var onceki = new { t.CariId, t.TeklfiTarihi, t.Durum };
            t.CariId = req.CariId;
            t.TeklfiTarihi = req.TeklifTarihi;
            t.Durum = req.Durum;
            TeklifHesap.Hesapla(t);
            await db.SaveChangesAsync();
            Audit.Log(db, nameof(Teklif), t.Id, "Güncellendi", onceki, new { t.CariId, t.TeklfiTarihi, t.Durum }, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.NoContent();
        });
        g.MapDelete("/{id:int}", async (int id, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();
            if (!IsEditable(t)) return Results.Conflict(new { error = "Teklif durumu nedeniyle silinemez" });
            var uid = GetUserId(ctx); var isAdmin = IsAdmin(ctx);
            if (!isAdmin && t.CreatedByUserId.HasValue && t.CreatedByUserId != uid) return Results.StatusCode(StatusCodes.Status403Forbidden);
            db.Set<Teklif>().Remove(t);
            await db.SaveChangesAsync();
            Audit.Log(db, nameof(Teklif), id, "Silindi", t, null, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.NoContent();
        });

        // Durum geçişleri
        g.MapPost("/{id:int}/status", async (int id, ChangeStatusRequest req, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();

            var uid = GetUserId(ctx); var isAdmin = IsAdmin(ctx);
            var current = t.Durum;
            var next = req.Durum;

            bool Allowed(string from, params string[] to) => current == from && to.Contains(next);

            // Kurallar
            if (Allowed("Taslak", "Iptal"))
            {
                // İptal: User kendi teklifini iptal edebilir, Admin tüm teklifleri iptal edebilir
                if (!isAdmin && (t.CreatedByUserId.HasValue && t.CreatedByUserId != uid))
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                t.Durum = "Iptal";
            }
            else if (Allowed("Taslak", "Kabul", "SureDoldu"))
            {
                // Admin: Taslak durumundaki teklifi direkt Onayla veya Süresi Doldu yapabilir
                if (!isAdmin) 
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                t.Durum = next;
            }
            else if (Allowed("Taslak", "Gonderildi"))
            {
                // Gönder işlemi: User kendi teklifini gönderebilir, Admin tüm teklifleri gönderebilir
                if (!isAdmin && (t.CreatedByUserId.HasValue && t.CreatedByUserId != uid))
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                
                if (t.Kalemler.Count == 0) return Results.BadRequest(new { error = "Gönderim için en az bir kalem gerekir" });
                if (t.TeklfiTarihi == default) return Results.BadRequest(new { error = "Gönderim için teklif tarihi gerekir" });
                t.Durum = "Gonderildi";
            }
            else if (Allowed("Gonderildi", "Kabul", "Red", "SureDoldu", "Iptal"))
            {
                // Gönderildi durumundan diğer durumlara geçiş: Sadece Admin
                if (!isAdmin) 
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                t.Durum = next;
            }
            else
            {
                return Results.Conflict(new { error = $"Geçersiz durum geçişi: {current} -> {next}" });
            }

            await db.SaveChangesAsync();
            Audit.Log(db, nameof(Teklif), t.Id, "Durum Değişti", new { OncekiDurum = current }, new { YeniDurum = t.Durum }, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.Ok(new { t.Id, t.Durum });
        });
        g.MapPost("/{id:int}/kalemler", async (int id, AddKalemRequest req, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();
            if (!IsEditable(t)) return Results.Conflict(new { error = "Teklif durumu nedeniyle düzenlenemez" });
            var uid = GetUserId(ctx); var isAdmin = IsAdmin(ctx);
            if (!isAdmin && t.CreatedByUserId.HasValue && t.CreatedByUserId != uid) return Results.StatusCode(StatusCodes.Status403Forbidden);
            var k = new TeklifKalem { TeklifId = id, StokId = req.StokId, Miktar = req.Miktar, BirimFiyat = req.BirimFiyat, IskontoOran = req.IskontoOran, KdvOran = req.KdvOran };
            t.Kalemler.Add(k);
            TeklifHesap.Hesapla(t);
            await db.SaveChangesAsync();
            Audit.Log(db, nameof(TeklifKalem), k.Id, "Oluşturuldu", null, k, uid);
            Audit.Log(db, nameof(Teklif), t.Id, "Guncellendi", null, new { t.AraToplam, t.GenelToplam }, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.Created($"/teklif/{t.Id}/kalemler/{k.Id}", new { k.Id });
        });
        g.MapPut("/{id:int}/kalemler/{kid:int}", async (int id, int kid, UpdateKalemRequest req, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();
            if (!IsEditable(t)) return Results.Conflict(new { error = "Teklif durumu nedeniyle düzenlenemez" });
            var uid = GetUserId(ctx); var isAdmin = IsAdmin(ctx);
            if (!isAdmin && t.CreatedByUserId.HasValue && t.CreatedByUserId != uid) return Results.StatusCode(StatusCodes.Status403Forbidden);
            var k = t.Kalemler.FirstOrDefault(x => x.Id == kid);
            if (k is null) return Results.NotFound();
            var onceki = new { k.Miktar, k.BirimFiyat, k.IskontoOran, k.KdvOran };
            k.Miktar = req.Miktar;
            k.BirimFiyat = req.BirimFiyat;
            k.IskontoOran = req.IskontoOran;
            k.KdvOran = req.KdvOran;
            TeklifHesap.Hesapla(t);
            await db.SaveChangesAsync();
            Audit.Log(db, nameof(TeklifKalem), k.Id, "Guncellendi", onceki, k, uid);
            Audit.Log(db, nameof(Teklif), t.Id, "Guncellendi", null, new { t.AraToplam, t.GenelToplam }, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.NoContent();
        });
        g.MapDelete("/{id:int}/kalem/{kid:int}", async (int id, int kid, AppDbContext db, HttpContext ctx) =>
        {
            using var tr = await db.Database.BeginTransactionAsync();
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();
            if (!IsEditable(t)) return Results.Conflict(new { error = "Teklif durumu nedeniyle silinemez" });
            var uid = GetUserId(ctx); var isAdmin = IsAdmin(ctx);
            if (!isAdmin && t.CreatedByUserId.HasValue && t.CreatedByUserId != uid) return Results.StatusCode(StatusCodes.Status403Forbidden);
            var k = t.Kalemler.FirstOrDefault(x => x.Id == kid);
            if (k is null) return Results.NotFound();
            t.Kalemler.Remove(k);
            TeklifHesap.Hesapla(t);
            await db.SaveChangesAsync();
            Audit.Log(db, nameof(TeklifKalem), kid, "Silindi", new { kid }, null, uid);
            Audit.Log(db, nameof(Teklif), t.Id, "Guncellendi", null, new { t.AraToplam, t.GenelToplam }, uid);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            return Results.NoContent();
        });
    }

    private static int? GetUserId(HttpContext ctx) 
    {
        var sub = ctx.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(sub, out var id) ? id : (int?)null;
    }

    private static bool IsAdmin(HttpContext ctx) => ctx.User?.IsInRole("Admin") == true;

    private static bool IsEditable(Teklif t)
        => t.Durum == "Taslak" || t.Durum == "Revizyonda";
}
