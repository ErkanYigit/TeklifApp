using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Data;
using Api.Contracts;
using System.Text;
using Application.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public static class KarsilastirmaActions
{
    public static void MapKarsilastirmaActions(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/karsilastirma/aksiyon").RequireAuthorization();


        // Seçilen satır(lar)ı sepete ekle
        g.MapPost("/sepet", async (IEnumerable<FiyatSatir> secilenler, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            var sepet = await db.Set<TeklifSepet>().FirstOrDefaultAsync(x => x.UserId == uid) ?? new TeklifSepet { UserId = uid.Value };
            if (sepet.Id == 0) db.Add(sepet);
            foreach (var s in secilenler)
                db.Add(new TeklifSepetKalem { SepetId = sepet.Id, StokId = s.StokId, Miktar = 1, HedefFiyat = s.Fiyat });
            await db.SaveChangesAsync();
            return Results.Ok();
        });
        // Doğrudan teklife kalem olarak ekle
        g.MapPost("/teklif/{teklifId:int}", async (int teklifId, IEnumerable<FiyatSatir> secilenler, AppDbContext db) =>
        {
            var t = await db.Set<Teklif>().Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == teklifId);
            if (t is null) return Results.NotFound();
            foreach (var s in secilenler)
                db.Add(new TeklifKalem { TeklifId = teklifId, StokId = s.StokId, Miktar = 1, BirimFiyat = s.Fiyat, IskontoOran = 0, KdvOran = 20 });
            await db.SaveChangesAsync();
            TeklifHesap.Hesapla(t); await db.SaveChangesAsync();
            return Results.Ok();
        });
        }

    private static int? GetUserId(HttpContext ctx)
    {
        var sub = ctx.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}