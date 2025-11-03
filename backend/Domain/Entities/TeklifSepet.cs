namespace Domain.Entities;

public class TeklifSepet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    public ICollection<TeklifSepetKalem> Kalemler { get; set; } = new List<TeklifSepetKalem>();
}