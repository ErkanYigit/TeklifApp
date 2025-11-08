public record KarsilastirmaQuery(
    int? StokId,
    int? FiyatListeNo,
    DateTime? Baslangic,
    DateTime? Bitis,
    int? CariId,
    string? TeklifDurum,
    int Page = 1,
    int PageSize = 50);

public record FiyatSatir(int StokId, string Kaynak, int? TedarikciId, string? TedarikciAd, int? TeklifId, string? TeklifNo, int? CariId, string? CariAd, decimal Fiyat, string PB, DateTime Tarih);
public record Ozet(decimal EnDusuk, decimal Medyan, decimal EnYuksek, DateTime? SonGuncelleme, int TedarikciSayisi, int TeklifSayisi);
public record KarsilastirmaYaniti(int StokId, string StokAd, IEnumerable<FiyatSatir> Satirlar, Ozet Ozet);
public record Paged<T>(IEnumerable<T> Items, int TotalCount);