namespace Domain.Entities;

public class Stok
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Birim { get; set; } = "Adet";
    public string? Cinsi { get; set; } 
    public bool Aktif { get; set; } = true;
    public ICollection<StokFiyat> Fiyatlar { get; set; } = new List<StokFiyat>();
}