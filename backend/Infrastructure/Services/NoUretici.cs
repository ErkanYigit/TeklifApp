using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Services;

public static class NoUretici
{
    public static string TeklifNo(AppDbContext db)
    {
        var yil = DateTime.UtcNow.Year;
        var prefix = $"TLF-{yil}-";
        var son = db.Set<Teklif>().Where(x => x.No.StartsWith(prefix))
        .OrderByDescending(x => x.No)
        .Select(x => x.No)
        .FirstOrDefault();
        var sayi = 0;
        if (!string.IsNullOrEmpty(son))
        {
            var parca = son.Split('-').Last();
            int.TryParse(parca, out sayi);
        }
        return $"{prefix}{(sayi + 1).ToString("D5")}";
    }
}