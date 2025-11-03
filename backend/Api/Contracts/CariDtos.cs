namespace Api.Contracts;

public record CariDto(int Id, string Kod, string Ad, string VergiNo, string VergiDairesi, string Telefon, string Eposta);
public record CreateCariRequest(string Kod, string Ad, string VergiNo, string VergiDairesi, string Telefon, string Eposta);
public record UpdateCariRequest(string Ad, string VergiNo, string VergiDairesi, string Telefon, string Eposta);


public record AdresDto(int Id, int CariId, string Tur, string Ulke, string Il, string Ilce, string Satir1, string? Satir2, string? PostaKodu);
public record CreateAdresRequest(string Tur, string Ulke, string Il, string Ilce, string Satir1, string? Satir2, string? PostaKodu);
public record UpdateAdresRequest(string Tur, string Ulke, string Il, string Ilce, string Satir1, string? Satir2, string? PostaKodu);


public record CariQuery(int Page = 1, int PageSize = 20, string? Search = null, string? Sort = "Ad", bool Desc = false);