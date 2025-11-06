namespace Api.Contracts;

public record TeklifDto(int Id, string No, int CariId, DateTime TeklfiTarihi, string Durum, decimal AraToplam, decimal IskontoToplam, decimal KdvToplam, decimal GenelToplam);
public record CreateTeklifRequest(int CariId, List<AddKalemRequest>? Kalemler);
public record UpdateTeklifRequest(int CariId, DateTime TeklifTarihi, string Durum);
public record ChangeStatusRequest(string Durum, string? Reason);

public record KalemDto(int Id, int TeklifId, int StokId, decimal Miktar, decimal BirimFiyat, decimal IskontoOran, decimal KdvOran, decimal Tutar, decimal IskontoTutar, decimal KdvTutar, decimal GenelTutar);
public record AddKalemRequest(int StokId, decimal Miktar, decimal BirimFiyat, decimal IskontoOran, decimal KdvOran);
public record UpdateKalemRequest(decimal Miktar, decimal BirimFiyat, decimal IskontoOran, decimal KdvOran);