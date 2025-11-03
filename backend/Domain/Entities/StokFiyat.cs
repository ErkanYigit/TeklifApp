namespace Domain.Entities;

public class StokFiyat
{
    public int Id { get; set; }
    public int StokId { get; set; }
    public int FiyatListeNo { get; set; } 
    public decimal Deger { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public DateTime YururlukTarihi { get; set; } = DateTime.UtcNow;
    public Stok Stok { get; set; } = null!;
}