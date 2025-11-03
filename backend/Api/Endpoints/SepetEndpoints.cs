using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Data;
using Api.Contracts;
using Application.Services;
using Infrastructure.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public static class SepetEndpoints
{
    public static void MapSepet(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/sepet").RequireAuthorization();
        
        g.MapGet("", async (AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx);
            if (uid is null) return Results.Unauthorized();
            
            var sepet = await db.Set<TeklifSepet>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.UserId == uid);
            if (sepet is null)
            {
                sepet = new TeklifSepet { UserId = uid.Value };
                db.Add(sepet);
                await db.SaveChangesAsync();
            }
            
            return Results.Ok(new
            {
                sepet = new SepetDto(sepet.Id, sepet.OlusturmaTarihi),
                kalemler = sepet.Kalemler.Select(k => new SepetKalemDto(k.Id, k.SepetId, k.StokId, k.Miktar, k.HedefFiyat)).ToList()
            });
        });
        
        g.MapPost("/kalemler", async (AddSepetKalemRequest req, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx);
            if (uid is null) return Results.Unauthorized();
            
            var sepet = await db.Set<TeklifSepet>().FirstOrDefaultAsync(x => x.UserId == uid);
            if (sepet is null)
            {
                sepet = new TeklifSepet { UserId = uid.Value };
                db.Add(sepet);
                await db.SaveChangesAsync();
            }
            
            var k = new TeklifSepetKalem
            {
                SepetId = sepet.Id,
                StokId = req.StokId,
                Miktar = req.Miktar,
                HedefFiyat = req.HedefFiyat
            };
            db.Add(k);
            await db.SaveChangesAsync();
            return Results.Created($"/sepet/kalem/{k.Id}", new { k.Id });
        });
        
        g.MapPut("/kalem/{kid:int}", async (int kid, UpdateSepetKalemRequest req, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx);
            if (uid is null) return Results.Unauthorized();
            
            var k = await db.Set<TeklifSepetKalem>().Include(x => x.Sepet).FirstOrDefaultAsync(x => x.Id == kid && x.Sepet.UserId == uid);
            if (k is null) return Results.NotFound();
            
            k.Miktar = req.Miktar;
            k.HedefFiyat = req.HedefFiyat;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
        
        g.MapDelete("/kalem/{kid:int}", async (int kid, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx);
            if (uid is null) return Results.Unauthorized();
            
            var k = await db.Set<TeklifSepetKalem>().Include(x => x.Sepet).FirstOrDefaultAsync(x => x.Id == kid && x.Sepet.UserId == uid);
            if (k is null) return Results.NotFound();
            
            db.Remove(k);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
        
        g.MapPost("/donustur", async (int cariId, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx);
            if (uid is null) return Results.Unauthorized();
            
            using var tr = await db.Database.BeginTransactionAsync();
            var sepet = await db.Set<TeklifSepet>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.UserId == uid);
            if (sepet is null || !sepet.Kalemler.Any()) return Results.BadRequest(new { error = "Sepet boş" });
            var cariVarMi = await db.Set<Cari>().AnyAsync(c => c.Id == cariId);
            if (!cariVarMi) return Results.BadRequest(new { error = "Geçersiz cari Id" });
            
            var t = new Teklif { CariId = cariId, No = NoUretici.TeklifNo(db), CreatedByUserId = uid };
            db.Add(t);
            await db.SaveChangesAsync();
            
            foreach (var s in sepet.Kalemler)
            {
                var bf = s.HedefFiyat ?? (await Infrastructure.Services.FiyatServisi.GetAktifFiyatAsync(db, s.StokId, null, null)) ?? 0m;
                db.Add(new TeklifKalem
                {
                    TeklifId = t.Id,
                    StokId = s.StokId,
                    Miktar = s.Miktar,
                    BirimFiyat = bf,
                    IskontoOran = 0,
                    KdvOran = 20
                });
            }
            await db.SaveChangesAsync();
            
            t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == t.Id);
            if (t is not null)
            {
                TeklifHesap.Hesapla(t);
                await db.SaveChangesAsync();
            }
            
            db.RemoveRange(sepet.Kalemler);
            await db.SaveChangesAsync();
            await tr.CommitAsync();
            
            return Results.Ok(new { t.Id, t.No });
        });
    }
    
    private static int? GetUserId(HttpContext ctx)
    {
        var sub = ctx.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}