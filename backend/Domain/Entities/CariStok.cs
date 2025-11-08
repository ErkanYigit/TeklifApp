namespace Domain.Entities;

public class CariStok
{
    public int Id { get; set; }
    public int CariId { get; set; }
    public Cari? Cari { get; set; }
    public int StokId { get; set; }
    public Stok? Stok { get; set; }
    public decimal? VarsayilanFiyat { get; set; }
    public string? ParaBirimi { get; set; }
    public decimal? IskontoOran { get; set; }
    public string? Not { get; set; }
    public bool Aktif { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
}

