using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Services;

public static class Audit
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void Log(AppDbContext db, string entity, int id, string aksiyon, object? onceki, object? sonraki, int? userId)
    {
        db.Set<AuditLog>().Add(new AuditLog
        {
            Entity = entity,
            EntityId = id,
            Aksiyon = aksiyon,
            UserId = userId,
            Onceki = onceki != null ? JsonSerializer.Serialize(onceki, JsonOptions) : null,
            Sonraki = sonraki != null ? JsonSerializer.Serialize(sonraki, JsonOptions) : null  
        });
    }
}