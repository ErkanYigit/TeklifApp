namespace Application.Services;

public static class Stats
{
    public static decimal Median(IEnumerable<decimal> values)
    {
        var arr = values.OrderBy(x => x).ToArray();
        if (arr.Length == 0) return 0m;
        var mid = arr.Length / 2;
        return arr.Length % 2 == 0 ? Math.Round((arr[mid-1] + arr[mid]) / 2m, 2) : arr[mid];
    }
}