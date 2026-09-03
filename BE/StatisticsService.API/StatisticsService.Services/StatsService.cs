using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StatisticsService.DTO.Responses;
using StatisticsService.Infrastructure.Exceptions;
using StatisticsService.Infrastructure.Repositories.Interfaces;
using StatisticsService.Infrastructure.Security;
using StatisticsService.Services.Interfaces;

namespace StatisticsService.Services;

public class StatsService(
    IStatsRepository  repo,
    IDistributedCache cache,
    ICurrentUser      currentUser
) : IStatsService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private static readonly HashSet<string> ValidGranularities = new() { "day", "week", "month", "year" };

    public async Task<IReadOnlyList<TimeseriesPointDto>> TimeseriesAsync(DateOnly? from, DateOnly? to, string? granularity)
    {
        var gran = NormalizeGranularity(granularity, fallback: "month");
        ValidateRange(from, to);
        return await GetOrSetAsync($"timeseries:{from}:{to}:{gran}", async () =>
            (await repo.TimeseriesAsync(currentUser.UserId, from, to, gran))
                .Select(r => new TimeseriesPointDto(r.Bucket, r.Kind, r.Total))
                .ToList());
    }

    public async Task<IReadOnlyList<CategorySliceDto>> CategoryBreakdownAsync(DateOnly? from, DateOnly? to, string kind)
    {
        var k = NormalizeKind(kind);
        ValidateRange(from, to);
        return await GetOrSetAsync($"breakdown:{from}:{to}:{k}", async () =>
            (await repo.CategoryBreakdownAsync(currentUser.UserId, from, to, k))
                .Select(r => new CategorySliceDto(r.CategoryId, r.CategoryName, r.Total, r.Cnt))
                .ToList());
    }

    public async Task<IReadOnlyList<TopCategoryDto>> TopCategoriesAsync(DateOnly? from, DateOnly? to, string kind, int? limit)
    {
        var k = NormalizeKind(kind);
        var lim = NormalizePositive(limit, fallback: 5, name: "limit");
        ValidateRange(from, to);
        return await GetOrSetAsync($"top:{from}:{to}:{k}:{lim}", async () =>
            (await repo.TopCategoriesAsync(currentUser.UserId, from, to, k, lim))
                .Select(r => new TopCategoryDto(r.CategoryName, r.Total, r.Pct))
                .ToList());
    }

    public async Task<IReadOnlyList<CalendarDayDto>> CalendarAsync(DateOnly? from, DateOnly? to)
    {
        // generate_series cere capete non-null. Fallback: ultimele 365 de zile pana azi.
        var toDate   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? toDate.AddDays(-365);
        ValidateRange(fromDate, toDate);
        return await GetOrSetAsync($"calendar:{fromDate}:{toDate}", async () =>
            (await repo.CalendarAsync(currentUser.UserId, fromDate, toDate))
                .Select(r => new CalendarDayDto(r.Zi, r.Cnt, r.Total))
                .ToList());
    }

    public async Task<IReadOnlyList<HistogramBucketDto>> HistogramAsync(DateOnly? from, DateOnly? to, decimal? max, int? buckets)
    {
        var mx = max ?? 1000m;
        if (mx <= 0) throw new ValidationException("Parametrul 'max' trebuie sa fie pozitiv.");
        var bk = NormalizePositive(buckets, fallback: 10, name: "buckets");
        ValidateRange(from, to);
        return await GetOrSetAsync($"histogram:{from}:{to}:{mx}:{bk}", async () =>
            (await repo.HistogramAsync(currentUser.UserId, from, to, mx, bk))
                .Select(r => new HistogramBucketDto(r.Bucket, r.Cnt))
                .ToList());
    }

    public async Task<IReadOnlyList<SavingsRateDto>> SavingsRateAsync(DateOnly? from, DateOnly? to)
    {
        ValidateRange(from, to);
        return await GetOrSetAsync($"savings:{from}:{to}", async () =>
            (await repo.SavingsRateAsync(currentUser.UserId, from, to))
                .Select(r => new SavingsRateDto(r.Luna, r.Venituri, r.Cheltuieli, r.Rata))
                .ToList());
    }

    public async Task<IReadOnlyList<RunningBalanceDto>> RunningBalanceAsync(DateOnly? from, DateOnly? to)
    {
        ValidateRange(from, to);
        return await GetOrSetAsync($"balance:{from}:{to}", async () =>
            (await repo.RunningBalanceAsync(currentUser.UserId, from, to))
                .Select(r => new RunningBalanceDto(r.Zi, r.SoldCumulat))
                .ToList());
    }

    public async Task<IReadOnlyList<MoMPointDto>> MoMAsync(DateOnly? from, DateOnly? to, string kind, string? granularity)
    {
        var k = NormalizeKind(kind);
        var gran = NormalizeGranularity(granularity, fallback: "month");
        if (gran is not ("month" or "year"))
            throw new ValidationException("Pentru MoM/YoY 'granularity' trebuie sa fie month sau year.");
        ValidateRange(from, to);
        return await GetOrSetAsync($"mom:{from}:{to}:{k}:{gran}", async () =>
            (await repo.MoMAsync(currentUser.UserId, from, to, k, gran))
                .Select(r => new MoMPointDto(r.Perioada, r.Total, r.TotalAnterior, r.VariatiePct))
                .ToList());
    }

    public async Task<IReadOnlyList<ParetoSliceDto>> ParetoAsync(DateOnly? from, DateOnly? to)
    {
        ValidateRange(from, to);
        return await GetOrSetAsync($"pareto:{from}:{to}", async () =>
            (await repo.ParetoAsync(currentUser.UserId, from, to))
                .Select(r => new ParetoSliceDto(r.CategoryName, r.Total, r.PctCumulat))
                .ToList());
    }

    public async Task<IReadOnlyList<WeekdayDto>> WeekdayAsync(DateOnly? from, DateOnly? to, string kind)
    {
        var k = NormalizeKind(kind);
        ValidateRange(from, to);
        return await GetOrSetAsync($"weekday:{from}:{to}:{k}", async () =>
            (await repo.WeekdayAsync(currentUser.UserId, from, to, k))
                .Select(r => new WeekdayDto(r.Dow, r.Zi, r.Total, r.Cnt))
                .ToList());
    }

    public async Task<IReadOnlyList<RecurringSplitDto>> RecurringSplitAsync(DateOnly? from, DateOnly? to, string kind)
    {
        var k = NormalizeKind(kind);
        ValidateRange(from, to);
        return await GetOrSetAsync($"recurring:{from}:{to}:{k}", async () =>
            (await repo.RecurringSplitAsync(currentUser.UserId, from, to, k))
                .Select(r => new RecurringSplitDto(r.EsteRecurenta, r.Total, r.Cnt))
                .ToList());
    }

    // ─── HELPERS ───────────────────────────────────────────────────────────────

    /// <summary>Read-through Redis: cheie per user+grafic+parametri, TTL 5 min, valoare JSON.</summary>
    private async Task<List<T>> GetOrSetAsync<T>(string suffix, Func<Task<List<T>>> factory)
    {
        var key = $"stats:{currentUser.UserId}:{suffix}";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<T>>(cached) ?? new List<T>();

        var result = await factory();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
        return result;
    }

    private static string NormalizeKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw new ValidationException("Parametrul 'kind' este obligatoriu (INCOME sau EXPENSE).");
        var upper = kind.Trim().ToUpperInvariant();
        if (upper is not ("INCOME" or "EXPENSE"))
            throw new ValidationException("Parametrul 'kind' trebuie sa fie INCOME sau EXPENSE.");
        return upper;
    }

    private static string NormalizeGranularity(string? granularity, string fallback)
    {
        if (string.IsNullOrWhiteSpace(granularity)) return fallback;
        var lower = granularity.Trim().ToLowerInvariant();
        if (!ValidGranularities.Contains(lower))
            throw new ValidationException("Parametrul 'granularity' trebuie sa fie day, week, month sau year.");
        return lower;
    }

    private static int NormalizePositive(int? value, int fallback, string name)
    {
        if (value is null) return fallback;
        if (value <= 0) throw new ValidationException($"Parametrul '{name}' trebuie sa fie pozitiv.");
        return value.Value;
    }

    private static void ValidateRange(DateOnly? from, DateOnly? to)
    {
        if (from is not null && to is not null && from > to)
            throw new ValidationException("Intervalul este invalid: 'from' trebuie sa fie <= 'to'.");
    }
}
