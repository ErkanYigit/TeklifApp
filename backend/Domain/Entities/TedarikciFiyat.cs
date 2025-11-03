namespace Domain.Entities;

public class TedarikciFiyat
{
    public int Id { get; set; }
    public int StokId { get; set; }
    public int TedarikciId { get; set; }
    public int FiyatListeNo { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public DateTime GuncellemeTarihi { get; set; } = DateTime.UtcNow;
}