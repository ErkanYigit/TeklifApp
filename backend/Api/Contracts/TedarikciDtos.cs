namespace Api.Contracts;

public record TedarikciDto(int Id, string Ad, string? Telefon, string? Eposta, bool Aktif);
public record CreateTedarikciRequest(string Ad, string? Telefon, string? Eposta, bool Aktif = true);
public record UpdateTedarikciRequest(string? Ad, string? Telefon, string? Eposta, bool? Aktif);

public record TedarikciQuery(int Page = 1, int PageSize = 20, string? Search = null, string? Sort = "Ad", bool Desc = false, bool? Aktif = null);

public record TedarikciFiyatDto(int Id, int StokId, int TedarikciId, string? TedarikciAd, int FiyatListeNo, decimal Fiyat, string ParaBirimi, DateTime GuncellemeTarihi);
public record CreateTedarikciFiyatRequest(int TedarikciId, int FiyatListeNo, decimal Fiyat, string ParaBirimi, DateTime? GuncellemeTarihi);
public record UpdateTedarikciFiyatRequest(int TedarikciId, int FiyatListeNo, decimal Fiyat, string ParaBirimi, DateTime? GuncellemeTarihi);

