using CurrencyService.Infrastructure.Frankfurter;

namespace CurrencyService.Infrastructure.Cache;

public interface IRateCache
{
    /// <summary>Întoarce tabelul de rate pentru base din cache; dacă lipsește, îl ia de la sursă și îl pune în cache (TTL 12h).</summary>
    Task<FrankfurterLatest> GetRatesAsync(string baseCode, CancellationToken ct = default);
}
