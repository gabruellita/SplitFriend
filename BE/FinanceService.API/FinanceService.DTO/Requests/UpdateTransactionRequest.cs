namespace FinanceService.DTO.Requests;

public record UpdateTransactionRequest(
    decimal  Amount,
    string   Kind,
    DateOnly TransactionDate,
    long?    CategoryId,
    long?    CurrencyId,
    string?  Description
);
