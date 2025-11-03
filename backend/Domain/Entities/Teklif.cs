namespace Domain.Entities;

public class Teklif
{
    public int Id { get; set; }
    public string No { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public int CariId { get; set; }
    public DateTime TeklfiTarihi { get; set; } = DateTime.UtcNow;
    public string Durum { get; set; } = "Taslak";
    public int? CreatedByUserId { get; set; }
    public decimal AraToplam { get; set; }
    public decimal IskontoToplam { get; set; }
    public decimal KdvToplam { get; set; }
    public decimal GenelToplam { get; set; }
    public ICollection<TeklifKalem> Kalemler { get; set; } = new List<TeklifKalem>();
}