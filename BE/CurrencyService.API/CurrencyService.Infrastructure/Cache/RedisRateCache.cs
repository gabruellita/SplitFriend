using System.Text.Json;
using CurrencyService.Infrastructure.Frankfurter;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyService.Infrastructure.Cache;

public class RedisRateCache(IDistributedCache cache, IFrankfurterClient frankfurter) : IRateCache
{
    private static readonly DistributedCacheEntryOptions Options = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
    };

    public async Task<FrankfurterLatest> GetRatesAsync(string baseCode, CancellationToken ct = default)
    {
        var key = $"fx:rates:{baseCode}";
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<FrankfurterLatest>(cached)!;

        var fresh = await frankfurter.GetLatestAsync(baseCode, ct);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(fresh), Options, ct);
        return fresh;
    }
}
