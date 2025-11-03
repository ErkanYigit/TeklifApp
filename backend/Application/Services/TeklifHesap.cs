using Domain.Entities;

namespace Application.Services;

public static class TeklifHesap
{
    public static void Hesapla(Teklif t)
    {
       t.AraToplam = 0; t.IskontoToplam = 0; t.KdvToplam = 0; t.GenelToplam = 0;
       foreach (var k in t.Kalemler)
       {
        k.Tutar = k.Miktar * k.BirimFiyat;
        k.IskontoTutar = Math.Round(k.Tutar * (k.IskontoOran / 100m), 2);
        var ara = k.Tutar - k.IskontoTutar;
        k.KdvTutar = Math.Round(ara * (k.KdvOran / 100m), 2);
        k.GenelTutar = ara + k.KdvTutar;
        t.AraToplam += k.Tutar;
        t.IskontoToplam += k.IskontoTutar;
        t.KdvToplam += k.KdvTutar;
        t.GenelToplam += k.GenelTutar;
       }
       t.AraToplam = Math.Round(t.AraToplam, 2);
       t.IskontoToplam = Math.Round(t.IskontoToplam, 2);
       t.KdvToplam = Math.Round(t.KdvToplam, 2);
       t.GenelToplam = Math.Round(t.GenelToplam, 2);
    }
}