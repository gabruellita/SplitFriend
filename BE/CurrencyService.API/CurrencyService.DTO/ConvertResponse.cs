namespace CurrencyService.DTO;

public record ConvertResponse(
    string From,
    string To,
    decimal Amount,
    decimal Rate,
    decimal Result,
    DateOnly Date
);
