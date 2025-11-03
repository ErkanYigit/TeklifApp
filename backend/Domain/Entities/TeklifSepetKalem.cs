namespace Domain.Entities;

public class TeklifSepetKalem
{
    public int Id { get; set; }
    public int SepetId { get; set; }
    public int StokId { get; set; }
    public decimal Miktar { get; set; }
    public decimal? HedefFiyat { get; set; } // opsiyonel: pazarlık hedefi
    public TeklifSepet Sepet { get; set; } = null!;
}