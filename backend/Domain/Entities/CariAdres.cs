namespace Domain.Entities;

public class CariAdres
{
    public int Id { get; set; }
    public int CariId { get; set; }
    public string Tur { get; set; } = "Fatura";
    public string Ulke { get; set; } = "TR";
    public string Il { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string Satir1 { get; set; } = string.Empty;
    public string? Satir2 { get; set; }
    public string? PostaKodu { get; set; }
    public Cari Cari { get; set; } = null!;
}