namespace Api.Contracts;

public record StokDto(int Id, string Kod, string Ad, string? Birim, string? Cins, bool Aktif);
public record CreateStokRequest(string Kod, string Ad, string? Birim, string? Cins, bool Aktif = true);
public record UpdateStokRequest(string? Ad, string? Birim, string? Cins, bool? Aktif);


public record StokFiyatDto(int Id, int StokId, int FiyatListeNo, decimal Deger, string ParaBirimi, DateTime YururlukTarihi);
public record CreateStokFiyatRequest(int FiyatListeNo, decimal Deger, string ParaBirimi, DateTime YururlukTarihi);
public record UpdateStokFiyatRequest(int FiyatListeNo, decimal Deger, string ParaBirimi, DateTime YururlukTarihi);


public record StokQuery(int Page = 1, int PageSize = 20, string? Search = null, string? Sort = "Ad", bool Desc = false, bool? Aktif = null);