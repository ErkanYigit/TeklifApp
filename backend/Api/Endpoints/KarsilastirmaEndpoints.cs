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
            var fiyatlar = db.Set<TedarikciFiyat>().AsNoTracking().Where(x => x.StokId == q.StokId);
            if (q.FiyatListeNo.HasValue) fiyatlar = fiyatlar.Where(x => x.FiyatListeNo == q.FiyatListeNo);
            if (q.Baslangic.HasValue) fiyatlar = fiyatlar.Where(x => x.GuncellemeTarihi >= q.Baslangic);
            if (q.Bitis.HasValue) fiyatlar = fiyatlar.Where(x => x.GuncellemeTarihi < q.Bitis);

            // Toplam kayıt sayısı
            var total = await fiyatlar.CountAsync();

            // Sayfalı sonuç + tedarikçi adı ile birleştir
            var pageQuery = fiyatlar
                .OrderBy(x => x.Fiyat)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize);

            var satirlar = await (from f in pageQuery
                                   join t in db.Set<Tedarikci>().AsNoTracking() on f.TedarikciId equals t.Id
                                   select new TedarikciSatir(
                                       f.TedarikciId,
                                       t.Ad,
                                       f.Fiyat,
                                       f.ParaBirimi,
                                       f.GuncellemeTarihi
                                   )).ToListAsync();

            var tum = await fiyatlar.Select(x => x.Fiyat).ToListAsync();
            var ozet = new Ozet(
                EnDusuk: tum.Count == 0 ? 0 : tum.Min(),
                Medyan: Stats.Median(tum),
                EnYuksek: tum.Count == 0 ? 0 : tum.Max(),
                SonGuncelleme: tum.Count == 0 ? (DateTime?)null : await fiyatlar.MaxAsync(x => (DateTime?)x.GuncellemeTarihi)
            );

            var stokAd = await db.Set<Stok>()
                .Where(s => s.Id == q.StokId)
                .Select(s => s.Ad)
                .FirstOrDefaultAsync() ?? "";

            return Results.Ok(new Paged<KarsilastirmaYaniti>(new[]
            {
                new KarsilastirmaYaniti(q.StokId!.Value, stokAd, satirlar, ozet)
            }, total));
        });
    }
}