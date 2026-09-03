using System.Text.Json;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace IdentityService.Infrastructure.Repositories;

// Decorator read-through peste CurrencyRepository. Cheia reala in Redis devine
// "IdentityService:currencies:list" (prefixul InstanceName setat in Program.cs).
public sealed class CachedCurrencyRepository(
    CurrencyRepository inner,
    IDistributedCache  cache) : ICurrencyRepository
{
    private const string CacheKey = "currencies:list";

    // Currencies sunt seed static (fara CRUD admin) -> TTL lung, fara invalidare.
    private static readonly DistributedCacheEntryOptions Options =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };

    public async Task<IEnumerable<Currency>> GetAllActiveAsync()
    {
        var cached = await cache.GetStringAsync(CacheKey);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<Currency>>(cached) ?? [];

        var fromDb = (await inner.GetAllActiveAsync()).ToList();
        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(fromDb), Options);
        return fromDb;
    }

    // ExistsActiveAsync ramane direct pe DB - validare punctuala, nu listare.
    public Task<bool> ExistsActiveAsync(long id) => inner.ExistsActiveAsync(id);
}
