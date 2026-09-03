namespace FinanceService.Infrastructure.Exchange;

public record FxConversion(string From, string To, decimal Amount, decimal Rate, decimal Result, DateOnly Date);

public interface IExchangeRateClient
{
    /// <summary>Convertește amount din moneda `from` în `to` la cursul curent (autoritar, server-side).</summary>
    Task<FxConversion> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default);
}
