namespace Api.Contracts;

public record FoyQuery(DateTime? Baslangic, DateTime? Bitis, int? CariId, string? Durum, string? Ara, int Page = 1, int PageSize = 20);
public record FoyItem(int Id, string No, int CariId, DateTime Tarih, string Durum, decimal GenelToplam);