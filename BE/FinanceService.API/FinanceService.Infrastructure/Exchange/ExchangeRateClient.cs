using System.Net.Http.Json;
using FinanceService.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace FinanceService.Infrastructure.Exchange;

public class ExchangeRateClient(HttpClient http, ILogger<ExchangeRateClient> logger) : IExchangeRateClient
{
    public async Task<FxConversion> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return new FxConversion(from, to, amount, 1m, decimal.Round(amount, 2), DateOnly.FromDateTime(DateTime.UtcNow));
        try
        {
            var amountStr = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var url = $"/api/currency/convert?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}&amount={amountStr}";
            var dto = await http.GetFromJsonAsync<FxConversion>(url, ct);
            if (dto is null) throw new ValidationException("Serviciul de curs nu a returnat date.");
            return dto;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "CurrencyService indisponibil ({From}->{To})", from, to);
            throw new ValidationException("Serviciul de curs valutar este indisponibil. Reîncearcă mai târziu.");
        }
        // Timeout (HttpClient.Timeout) => TaskCanceledException, nu HttpRequestException; il tratam tot ca indisponibilitate.
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "CurrencyService timeout ({From}->{To})", from, to);
            throw new ValidationException("Serviciul de curs valutar este indisponibil. Reîncearcă mai târziu.");
        }
    }
}
