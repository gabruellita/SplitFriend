namespace FinanceService.DTO.Responses;

public record PaymentResponse(
    long     Id,
    long     FromUserId,
    long     ToUserId,
    decimal  Amount,
    long     CurrencyId,
    string?  CurrencyCode,
    decimal  OriginalAmount,
    long     OriginalCurrencyId,
    string?  OriginalCurrencyCode,
    decimal  ExchangeRate,
    DateOnly RateDate,
    string?  PaymentMethod,
    DateTime PaidAt
);
