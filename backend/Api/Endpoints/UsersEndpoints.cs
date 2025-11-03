using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;

public static class UsersEndpoints
{
    public static void MapUsers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/users").RequireAuthorization("Admin");

        g.MapGet("", async (int page, int pageSize, string? search, AppDbContext db) =>
        {
            var qry = db.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                qry = qry.Where(u => u.UserCode.Contains(search) || u.Email.Contains(search) || u.UserName.Contains(search));
            var total = await qry.CountAsync();
            var items = await qry.OrderBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new { u.Id, u.UserCode, u.UserName, u.Email, u.Role, u.Active })
                .ToListAsync();
            return Results.Ok(new { items, total });
        });

        g.MapPost("/{id:int}/set-role", async (int id, string role, AppDbContext db) =>
        {
            var u = await db.Users.FindAsync(id); if (u is null) return Results.NotFound();
            u.Role = role; await db.SaveChangesAsync();
            return Results.Ok(new { message = "Rol güncellendi" });
        });

        g.MapPost("/{id:int}/activate", async (int id, AppDbContext db) =>
        {
            var u = await db.Users.FindAsync(id); if (u is null) return Results.NotFound();
            u.Active = true; await db.SaveChangesAsync();
            return Results.Ok(new { message = "Kullanıcı aktifleştirildi" });
        });

        g.MapPost("/{id:int}/deactivate", async (int id, AppDbContext db) =>
        {
            var u = await db.Users.FindAsync(id); if (u is null) return Results.NotFound();
            u.Active = false; await db.SaveChangesAsync();
            return Results.Ok(new { message = "Kullanıcı pasifleştirildi" });
        });

        g.MapPost("/{id:int}/reset-password", async (int id, string newPassword, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return Results.BadRequest(new { error = "Yeni şifre en az 6 karakter olmalı" });
            var u = await db.Users.FindAsync(id); if (u is null) return Results.NotFound();
            u.Password = Password.Hash(newPassword);
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Şifre sıfırlandı" });
        });

        g.MapPost("/{id:int}/revoke-refresh", async (int id, AppDbContext db) =>
        {
            var tokens = await db.RefreshTokens.Where(r => r.UserId == id && !r.Revoked).ToListAsync();
            foreach (var t in tokens) t.Revoked = true;
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Tüm refresh tokenlar iptal edildi" });
        });
    }
}


