namespace Api.Contracts;

public record SepetDto(int Id, DateTime OlusturmaTarihi);
public record SepetKalemDto(int Id, int SepetId, int StokId, decimal Miktar, decimal? HedefFiyat);
public record AddSepetKalemRequest(int StokId, decimal Miktar, decimal? HedefFiyat);
public record UpdateSepetKalemRequest(decimal Miktar, decimal? HedefFiyat);
