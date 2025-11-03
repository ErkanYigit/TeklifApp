namespace Domain.Entities;

public class Tedarikci
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Eposta { get; set; }
    public bool Aktif { get; set; } = true;
}