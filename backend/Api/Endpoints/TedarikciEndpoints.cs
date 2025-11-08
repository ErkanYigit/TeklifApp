using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using Api.Contracts;

public static class TedarikciEndpoints
{
    public static void MapTedarikci(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/tedarikci").RequireAuthorization();

        g.MapGet("", async ([AsParameters] TedarikciQuery q, AppDbContext db) =>
        {
            var qry = db.Set<Tedarikci>().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q.Search))
                qry = qry.Where(x => x.Ad.Contains(q.Search));
            if (q.Aktif.HasValue) qry = qry.Where(x => x.Aktif == q.Aktif);
            qry = (q.Sort ?? "Ad").ToLower() switch
            {
                _ => (q.Desc ? qry.OrderByDescending(x => x.Ad) : qry.OrderBy(x => x.Ad))
            };
            var total = await qry.CountAsync();
            var items = await qry.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(x => new TedarikciDto(x.Id, x.Ad, x.Telefon, x.Eposta, x.Aktif)).ToListAsync();
            return Results.Ok(new Paged<TedarikciDto>(items, total));
        });

        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var tedarikci = await db.Set<Tedarikci>().FirstOrDefaultAsync(x => x.Id == id);
            if (tedarikci is null) return Results.NotFound();
            return Results.Ok(new TedarikciDto(tedarikci.Id, tedarikci.Ad, tedarikci.Telefon, tedarikci.Eposta, tedarikci.Aktif));
        });

        g.MapPost("", async (CreateTedarikciRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Ad)) return Results.BadRequest(new { error = "Ad zorunlu" });
            req = req with { Ad = req.Ad?.Trim() ?? "" };
            if (await db.Set<Tedarikci>().AnyAsync(x => x.Ad == req.Ad))
                return Results.Conflict(new { error = "Bu isimde tedarikçi zaten kayıtlı" });
            var tedarikci = new Tedarikci { Ad = req.Ad, Telefon = req.Telefon?.Trim(), Eposta = req.Eposta?.Trim(), Aktif = req.Aktif };
            db.Add(tedarikci);
            await db.SaveChangesAsync();
            return Results.Created($"/tedarikci/{tedarikci.Id}", new { tedarikci.Id });
        }).RequireAuthorization("AdminOrPurchase");

        g.MapPut("/{id:int}", async (int id, UpdateTedarikciRequest req, AppDbContext db) =>
        {
            var tedarikci = await db.Set<Tedarikci>().FindAsync(id);
            if (tedarikci is null) return Results.NotFound();
            if (req.Ad is not null)
            {
                if (string.IsNullOrWhiteSpace(req.Ad)) return Results.BadRequest(new { error = "Ad zorunlu" });
                tedarikci.Ad = req.Ad.Trim();
            }
            if (req.Telefon is not null) tedarikci.Telefon = req.Telefon?.Trim();
            if (req.Eposta is not null) tedarikci.Eposta = req.Eposta?.Trim();
            if (req.Aktif.HasValue) tedarikci.Aktif = req.Aktif.Value;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var tedarikci = await db.Set<Tedarikci>().FindAsync(id);
            if (tedarikci is null) return Results.NotFound();
            var hasFiyat = await db.Set<TedarikciFiyat>().AnyAsync(x => x.TedarikciId == id);
            if (hasFiyat) return Results.Conflict(new { error = "Tedarikçiye bağlı fiyat kayıtları olduğu için silinemez" });
            db.Remove(tedarikci);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOrPurchase");
    }
}

