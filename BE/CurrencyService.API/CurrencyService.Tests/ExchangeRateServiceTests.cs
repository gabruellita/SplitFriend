using CurrencyService.Infrastructure.Cache;
using CurrencyService.Infrastructure.Exceptions;
using CurrencyService.Infrastructure.Frankfurter;
using CurrencyService.Services;

namespace CurrencyService.Tests;

/// <summary>
/// Teste pentru conversia valutara (ExchangeRateService, sectiunea 4.5.6).
/// Sursa de rate (cache-ul) este inlocuita cu un fake in memorie — fara Redis sau retea.
/// </summary>
public class ExchangeRateServiceTests
{
    private sealed class FakeRateCache(FrankfurterLatest snapshot) : IRateCache
    {
        public Task<FrankfurterLatest> GetRatesAsync(string baseCode, CancellationToken ct = default)
            => Task.FromResult(snapshot);
    }

    private static ExchangeRateService Service(params (string code, decimal rate)[] rates)
    {
        var snap = new FrankfurterLatest("EUR", new DateOnly(2026, 6, 16),
            rates.ToDictionary(r => r.code, r => r.rate));
        return new ExchangeRateService(new FakeRateCache(snap));
    }

    [Fact]
    public async Task Convert_AceeasiMoneda_RataEste1_FaraApelLaSursa()
    {
        var svc = Service(); // niciun rate necesar
        var res = await svc.ConvertAsync("RON", "RON", 100m);

        Assert.Equal(1m, res.Rate);
        Assert.Equal(100m, res.Result);
        Assert.Equal("RON", res.From);
        Assert.Equal("RON", res.To);
    }

    [Fact]
    public async Task Convert_CuRata_AplicaRotunjireaLaDoiZecimali()
    {
        var svc = Service(("RON", 4.9772m));
        var res = await svc.ConvertAsync("EUR", "RON", 10m);

        Assert.Equal(4.9772m, res.Rate);
        Assert.Equal(49.77m, res.Result); // 49,772 -> 49,77
    }

    [Fact]
    public async Task Convert_MonedaTintaNesuportata_AruncaCurrencyException()
    {
        var svc = Service(("RON", 4.97m)); // USD lipseste
        await Assert.ThrowsAsync<CurrencyException>(() => svc.ConvertAsync("EUR", "USD", 10m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Convert_SumaNepozitiva_AruncaCurrencyException(decimal amount)
    {
        var svc = Service(("RON", 4.97m));
        await Assert.ThrowsAsync<CurrencyException>(() => svc.ConvertAsync("EUR", "RON", amount));
    }

    [Theory]
    [InlineData("EU")]     // prea scurt
    [InlineData("EURO")]   // prea lung
    [InlineData("E1R")]    // contine cifra
    public async Task Convert_CodInvalid_AruncaCurrencyException(string code)
    {
        var svc = Service(("RON", 4.97m));
        await Assert.ThrowsAsync<CurrencyException>(() => svc.ConvertAsync(code, "RON", 10m));
    }

    [Fact]
    public async Task Convert_NormalizeazaCodul_LowercaseSiSpatii()
    {
        var svc = Service(("RON", 4.9772m));
        var res = await svc.ConvertAsync("  eur ", "ron", 10m); // se normalizeaza la EUR/RON

        Assert.Equal("EUR", res.From);
        Assert.Equal("RON", res.To);
        Assert.Equal(49.77m, res.Result);
    }

    [Fact]
    public async Task GetRates_IntoarceTabelulDinSursa()
    {
        var svc = Service(("RON", 4.97m), ("USD", 1.08m));
        var res = await svc.GetRatesAsync("eur");

        Assert.Equal("EUR", res.Base);
        Assert.Equal(2, res.Rates.Count);
        Assert.Equal(4.97m, res.Rates["RON"]);
    }
}
