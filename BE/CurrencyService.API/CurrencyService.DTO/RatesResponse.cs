namespace CurrencyService.DTO;

public record RatesResponse(
    string Base,
    DateOnly Date,
    IReadOnlyDictionary<string, decimal> Rates
);
