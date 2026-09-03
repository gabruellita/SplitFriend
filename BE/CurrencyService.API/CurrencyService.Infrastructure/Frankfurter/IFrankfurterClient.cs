namespace CurrencyService.Infrastructure.Frankfurter;

public record FrankfurterLatest(string Base, DateOnly Date, IReadOnlyDictionary<string, decimal> Rates);

public interface IFrankfurterClient
{
    /// <summary>GET /latest?base={base} — toate ratele față de moneda de bază.</summary>
    Task<FrankfurterLatest> GetLatestAsync(string baseCode, CancellationToken ct = default);
}
