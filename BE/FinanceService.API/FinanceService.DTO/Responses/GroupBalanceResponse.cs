namespace FinanceService.DTO.Responses;

public record GroupBalanceResponse(
    long    UserId,
    string? Username,
    long    CurrencyId,
    string? CurrencyCode,
    decimal NetAmount   // + grupul ii datoreaza; − el datoreaza grupului
);
