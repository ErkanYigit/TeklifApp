using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;  
using Infrastructure.Data;
using Api.Contracts;
using System.Text;
using Application.Services;

public static class KarsilastirmaEndpoints
{
    public static void MapKarsilastirma(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/karsilastirma").RequireAuthorization();

        g.MapGet("", async ([AsParameters] KarsilastirmaQuery q, AppDbContext db) =>
        {
            if (!q.StokId.HasValue) return Results.BadRequest("StokId gerekli");
            
            var stokAd = await db.Set<Stok>()
                .Where(s => s.Id == q.StokId)
                .Select(s => s.Ad)
                .FirstOrDefaultAsync() ?? "";

            // Tedarikçi fiyatları - Sadece tedarikçi fiyatlarını göster (maliyet karşılaştırması)
            var tedarikciFiyatlar = db.Set<TedarikciFiyat>().AsNoTracking().Where(x => x.StokId == q.StokId);
            if (q.FiyatListeNo.HasValue) tedarikciFiyatlar = tedarikciFiyatlar.Where(x => x.FiyatListeNo == q.FiyatListeNo);
            if (q.Baslangic.HasValue) tedarikciFiyatlar = tedarikciFiyatlar.Where(x => x.GuncellemeTarihi >= q.Baslangic);
            if (q.Bitis.HasValue) tedarikciFiyatlar = tedarikciFiyatlar.Where(x => x.GuncellemeTarihi < q.Bitis);

            var tedarikciSatirlar = await (from f in tedarikciFiyatlar
                                           join t in db.Set<Tedarikci>().AsNoTracking() on f.TedarikciId equals t.Id
                                           select new FiyatSatir(
                                               q.StokId!.Value,
                                               "Tedarikci",
                                               f.TedarikciId,
                                               t.Ad,
                                               null,
                                               null,
                                               null,
                                               null,
                                               f.Fiyat,
                                               f.ParaBirimi,
                                               f.GuncellemeTarihi
                                           )).ToListAsync();

            // Sadece tedarikçi fiyatlarını kullan (müşteri teklifleri değil)
            var tumSatirlar = tedarikciSatirlar
                .OrderBy(x => x.Fiyat)
                .ToList();

            // Sayfalama
            var total = tumSatirlar.Count;
            var sayfaliSatirlar = tumSatirlar
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToList();

            // Tüm fiyatlar üzerinden özet (sadece tedarikçi fiyatları)
            var tumFiyatlar = tumSatirlar.Select(x => x.Fiyat).ToList();
            var ozet = new Ozet(
                EnDusuk: tumFiyatlar.Count == 0 ? 0 : tumFiyatlar.Min(),
                Medyan: Stats.Median(tumFiyatlar),
                EnYuksek: tumFiyatlar.Count == 0 ? 0 : tumFiyatlar.Max(),
                SonGuncelleme: tumSatirlar.Count == 0 ? null : tumSatirlar.Max(x => (DateTime?)x.Tarih),
                TedarikciSayisi: tedarikciSatirlar.Count,
                TeklifSayisi: 0 // Artık teklif gösterilmiyor, sadece tedarikçi fiyatları
            );

            return Results.Ok(new Paged<KarsilastirmaYaniti>(new[]
            {
                new KarsilastirmaYaniti(q.StokId!.Value, stokAd, sayfaliSatirlar, ozet)
            }, total));
        });
    }
}