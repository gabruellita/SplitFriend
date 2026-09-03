using CurrencyService.DTO;
using CurrencyService.Infrastructure.Cache;
using CurrencyService.Infrastructure.Exceptions;
using CurrencyService.Services.Interfaces;

namespace CurrencyService.Services;

public class ExchangeRateService(IRateCache cache) : IExchangeRateService
{
    public async Task<RatesResponse> GetRatesAsync(string baseCode, CancellationToken ct = default)
    {
        var b = Normalize(baseCode);
        var latest = await cache.GetRatesAsync(b, ct);
        return new RatesResponse(latest.Base, latest.Date, latest.Rates);
    }

    public async Task<ConvertResponse> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        var f = Normalize(from);
        var t = Normalize(to);
        if (amount <= 0)
            throw new CurrencyException("Suma trebuie să fie pozitivă.");

        if (f == t)
            return new ConvertResponse(f, t, amount, 1m, decimal.Round(amount, 2), DateOnly.FromDateTime(DateTime.UtcNow));

        var latest = await cache.GetRatesAsync(f, ct);
        if (!latest.Rates.TryGetValue(t, out var rate))
            throw new CurrencyException($"Moneda '{t}' nu este suportată de sursa de curs.");

        var result = decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        return new ConvertResponse(f, t, amount, rate, result, latest.Date);
    }

    private static string Normalize(string code)
    {
        var c = code?.Trim().ToUpperInvariant() ?? "";
        if (c.Length != 3 || !c.All(char.IsLetter))
            throw new CurrencyException("Codul de monedă trebuie să aibă 3 litere (ex. EUR).");
        return c;
    }
}
