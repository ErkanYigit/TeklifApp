using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public static class FiyatServisi
{
    public static async Task<decimal?> GetAktifFiyatAsync(AppDbContext db, int stokId, int? fiyatListeNo, DateTime? tarih)
    {
        var t = tarih ?? DateTime.UtcNow;
        var q = db.Set<Domain.Entities.StokFiyat>().AsNoTracking()
            .Where(x => x.StokId == stokId && x.YururlukTarihi <= t);
        if (fiyatListeNo.HasValue)
            q = q.Where(x => x.FiyatListeNo == fiyatListeNo.Value);
        var f = await q.OrderByDescending(x => x.YururlukTarihi).Select(x => (decimal?)x.Deger).FirstOrDefaultAsync();
        return f;
    }
}


