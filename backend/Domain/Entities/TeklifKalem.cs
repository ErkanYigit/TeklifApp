namespace Domain.Entities;

public class TeklifKalem
{
    public int Id { get; set; }
    public int TeklifId { get; set; }
    public Teklif? Teklif { get; set; }
    public int StokId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal IskontoOran { get; set; }
    public decimal KdvOran { get; set; }
    public decimal Tutar { get; set; }
    public decimal IskontoTutar { get; set; }
    public decimal KdvTutar { get; set; }
    public decimal GenelTutar { get; set; }
}
