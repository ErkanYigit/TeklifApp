namespace Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime Zaman { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public string Entity { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Aksiyon { get; set; } = string.Empty;
    public string? Onceki {get; set; }
    public string? Sonraki {get; set; }
}