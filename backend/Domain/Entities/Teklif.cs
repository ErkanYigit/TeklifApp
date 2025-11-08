namespace Domain.Entities;

public class Teklif
{
    public int Id { get; set; }
    public string No { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public int CariId { get; set; }
    public Cari? Cari { get; set; }
    public DateTime TeklfiTarihi { get; set; } = DateTime.UtcNow;
    public string Durum { get; set; } = "Taslak";
    public int? CreatedByUserId { get; set; }
    public decimal AraToplam { get; set; }
    public decimal IskontoToplam { get; set; }
    public decimal KdvToplam { get; set; }
    public decimal GenelToplam { get; set; }
    public string? OnayToken { get; set; }
    public DateTime? OnayTokenGecerlilik { get; set; }
    public DateTime? OnayZamani { get; set; }
    public string? OnaylayanAd { get; set; }
    public DateTime? RedZamani { get; set; }
    public string? RedNotu { get; set; }
    public ICollection<TeklifKalem> Kalemler { get; set; } = new List<TeklifKalem>();
}