using CurrencyService.DTO;

namespace CurrencyService.Services.Interfaces;

public interface IExchangeRateService
{
    Task<RatesResponse> GetRatesAsync(string baseCode, CancellationToken ct = default);
    Task<ConvertResponse> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default);
}
