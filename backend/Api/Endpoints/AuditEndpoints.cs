using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

public static class AuditEndpoints
{
    public static void MapAudit(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/audit").RequireAuthorization("Admin");

        g.MapGet("", async (int page, int pageSize, string? entity, int? entityId, AppDbContext db) =>
        {
            var qry = db.AuditLogs.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(entity)) qry = qry.Where(a => a.Entity == entity);
            if (entityId.HasValue) qry = qry.Where(a => a.EntityId == entityId);
            var total = await qry.CountAsync();
            var items = await qry.OrderByDescending(a => a.Zaman)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new { a.Id, Entity = a.Entity, a.EntityId, Action = a.Aksiyon, OldValues = a.Onceki, NewValues = a.Sonraki, CreatedAt = a.Zaman, a.UserId })
                .ToListAsync();
            return Results.Ok(new { items, total });
        });
    }
}


