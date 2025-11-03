public record KarsilastirmaQuery(
    int? StokId,
    int? FiyatListeNo,
    DateTime? Baslangic,
    DateTime? Bitis,
    int Page = 1,
    int PageSize = 50);

public record TedarikciSatir(int TedarikciId, string TedarikciAd, decimal Fiyat, string PB, DateTime Guncelleme);
public record Ozet(decimal EnDusuk, decimal Medyan, decimal EnYuksek, DateTime? SonGuncelleme);
public record KarsilastirmaYaniti(int StokId, string StokAd, IEnumerable<TedarikciSatir> Satirlar, Ozet Ozet);
public record Paged<T>(IEnumerable<T> Items, int TotalCount);