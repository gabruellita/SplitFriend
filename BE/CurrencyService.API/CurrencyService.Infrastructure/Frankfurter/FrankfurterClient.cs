using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CurrencyService.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace CurrencyService.Infrastructure.Frankfurter;

public class FrankfurterClient(HttpClient http, ILogger<FrankfurterClient> logger) : IFrankfurterClient
{
    private sealed record Dto(
        [property: JsonPropertyName("base")] string Base,
        [property: JsonPropertyName("date")] string Date,
        [property: JsonPropertyName("rates")] Dictionary<string, decimal> Rates);

    public async Task<FrankfurterLatest> GetLatestAsync(string baseCode, CancellationToken ct = default)
    {
        try
        {
            // base address se termina cu /v1/ → calea relativa NU incepe cu '/' ca sa nu suprascrie /v1
            var dto = await http.GetFromJsonAsync<Dto>($"latest?base={baseCode}", ct);
            if (dto is null)
                throw new CurrencyException("Sursa de curs nu a returnat date.", 503);
            return new FrankfurterLatest(dto.Base, DateOnly.Parse(dto.Date), dto.Rates);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Frankfurter indisponibil pentru base {Base}", baseCode);
            throw new CurrencyException("Sursa de curs este indisponibilă momentan.", 503);
        }
    }
}
