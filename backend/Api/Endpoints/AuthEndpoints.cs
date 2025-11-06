using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Api.Utils;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterRequest req, AppDbContext db) =>
        {
            // Telefon numarası zorunlu ve geçerli olmalı (yeni kayıtlar için)
            if (string.IsNullOrWhiteSpace(req.Phone))
                return Results.BadRequest(new { error = "Telefon numarası zorunludur" });

            // Telefon numarasını doğrula ve normalize et
            if (!PhoneValidator.IsValidTurkishPhone(req.Phone, out var normalizedPhone))
                return Results.BadRequest(new { error = "Geçersiz telefon numarası formatı. Lütfen Türkiye telefon numarası formatında girin (örn: 05XX XXX XX XX)" });

            // Email, kullanıcı kodu ve telefon kontrolü
            if (await db.Users.AnyAsync(x => x.Email == req.Email || x.UserCode == req.UserCode))
                return Results.BadRequest(new { error = "Email veya kullanıcı kodu zaten kullanılıyor" });

            if (await db.Users.AnyAsync(x => x.Phone == normalizedPhone))
                return Results.BadRequest(new { error = "Bu telefon numarası zaten kayıtlı" });

            try
            {
                var user = new User
                {
                    Email = req.Email.Trim(),
                    UserCode = req.UserCode.Trim(),
                    UserName = req.UserCode.Trim(), // UserName unique indexi için doldur
                    Password = Password.Hash(req.Password),
                    Phone = normalizedPhone!,
                    Active = true,
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Kullanıcı başarıyla oluşturuldu" });
            }
            catch (DbUpdateException ex)
            {
                // Unique ihlalleri 400 döndür
                return Results.BadRequest(new { error = "Kullanıcı oluşturulamadı. Email/kod/telefon zaten kayıtlı olabilir.", detail = ex.InnerException?.Message ?? ex.Message });
            }
        });

        app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db, IConfiguration cfg) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserCode == req.UserCode);
            if (user is null)
                return Results.NotFound(new { error = "Kullanıcı bulunamadı" });
            if (!Password.Verify(req.Password, user.Password))
                return Results.Json(new { error = "Kullanıcı adı veya şifre hatalı" }, statusCode: StatusCodes.Status401Unauthorized);
            if (!user.Active)
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            var jwt = new JwtService(cfg);
            var (token, expiresAt) = jwt.CreateToken(user);
            var daysStr = cfg["Jwt:RefreshTokenDays"]; int days;
            if (!int.TryParse(daysStr, out days) || days <= 0) days = 30; // varsayılan 30 gün
            var refresh = new RefreshToken { Token = JwtService.GenerateRefreshToken(), Expires = DateTime.UtcNow.AddDays(days), UserId = user.Id };
            db.RefreshTokens.Add(refresh);
            await db.SaveChangesAsync();

            return Results.Ok(new TokenResponse(token, refresh.Token, expiresAt, user.Role));
        });

        app.MapPost("/auth/refresh", async (string refreshToken, AppDbContext db, IConfiguration cfg) =>
        {
            var rt = await db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshToken && !x.Revoked);
            if (rt is null || rt.Expires < DateTime.UtcNow) return Results.Unauthorized();
            var jwt = new JwtService(cfg);
            var (token, expiresAt) = jwt.CreateToken(rt.User);
            return Results.Ok(new TokenResponse(token, rt.Token, expiresAt, rt.User.Role));
        });

        app.MapPost("/auth/logout", async (string refreshToken, AppDbContext db) =>
        {
            var rt = await db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshToken && !x.Revoked);
            if (rt is null) return Results.Ok();
            rt.Revoked = true;
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Başarıyla çıkış yapıldı" });
        }).RequireAuthorization();

        // Me - profil görüntüleme
        app.MapGet("/users/me", async (AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == uid);
            if (u is null) return Results.NotFound();
            return Results.Ok(new { u.Id, u.UserCode, u.UserName, u.Email, u.Role, u.Active });
        }).RequireAuthorization();

        // Me - profil güncelleme (ad/eposta)
        app.MapPut("/users/me", async (UpdateMeRequest req, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == uid);
            if (u is null) return Results.NotFound();
            if (!string.IsNullOrWhiteSpace(req.Email)) u.Email = req.Email.Trim();
            if (!string.IsNullOrWhiteSpace(req.UserName)) u.UserName = req.UserName.Trim();
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Me - şifre değiştirme
        app.MapPost("/auth/change-password", async (ChangePasswordRequest req, AppDbContext db, HttpContext ctx) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            var u = await db.Users.FirstOrDefaultAsync(x => x.Id == uid);
            if (u is null) return Results.NotFound();
            if (!Password.Verify(req.OldPassword, u.Password))
                return Results.BadRequest(new { error = "Mevcut şifre yanlış" });
            if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
                return Results.BadRequest(new { error = "Yeni şifre en az 6 karakter olmalı" });
            u.Password = Password.Hash(req.NewPassword);
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Şifre güncellendi" });
        }).RequireAuthorization();

        // Admin rol atama (sadece Admin erişebilir)
        app.MapPost("/auth/make-admin", async (string userCode, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(userCode)) return Results.BadRequest(new { error = "userCode gerekli" });
            var u = await db.Users.FirstOrDefaultAsync(x => x.UserCode == userCode);
            if (u is null) return Results.NotFound(new { error = "Kullanıcı bulunamadı" });
            u.Role = "Admin";
            u.Active = true;
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Kullanıcı admin yapıldı", user = new { u.Id, u.UserCode, u.Email, u.Role } });
        }).RequireAuthorization("Admin");

        // İlk admin kurulumu: sistemde hiç admin yoksa, herkes çağırabilir
        app.MapPost("/auth/bootstrap-admin", async (string userCode, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(userCode)) return Results.BadRequest(new { error = "userCode gerekli" });
            var alreadyAdmin = await db.Users.AnyAsync(x => x.Role == "Admin");
            if (alreadyAdmin) return Results.StatusCode(StatusCodes.Status403Forbidden);
            var u = await db.Users.FirstOrDefaultAsync(x => x.UserCode == userCode);
            if (u is null) return Results.NotFound(new { error = "Kullanıcı bulunamadı" });
            u.Role = "Admin"; u.Active = true; await db.SaveChangesAsync();
            return Results.Ok(new { message = "İlk admin atandı", user = new { u.Id, u.UserCode, u.Email, u.Role } });
        });
    }

    private static int? GetUserId(HttpContext ctx)
    {
        var sub = ctx.User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}

public record UpdateMeRequest(string? UserName, string? Email);
public record ChangePasswordRequest(string OldPassword, string NewPassword);