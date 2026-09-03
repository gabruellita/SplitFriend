namespace FinanceService.DTO.Requests;

public record CreateTransactionRequest(
    decimal  Amount,
    string   Kind,             // "INCOME" / "EXPENSE"
    DateOnly TransactionDate,
    long?    CategoryId,
    long?    CurrencyId,       // optional — fallback pe moneda preferata a userului
    string?  Description
);
