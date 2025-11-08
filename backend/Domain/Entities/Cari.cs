namespace Domain.Entities;

public class Cari
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string VergiNo { get; set; } = string.Empty;
    public string VergiDairesi { get; set; } = string.Empty; 
    public string Telefon { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
    public ICollection<CariAdres> Adresler { get; set; } = new List<CariAdres>();
}