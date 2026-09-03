namespace FinanceService.DTO.Responses;

public record GroupExpenseResponse(
    long                              Id,
    long                              GroupId,
    long                              PaidByUserId,
    string                            Title,
    decimal                           Amount,
    long                              CurrencyId,
    string?                           CurrencyCode,
    string                            SplitType,
    string                            Status,
    DateOnly                          ExpenseDate,
    DateTime                          CreatedAt,
    IReadOnlyList<ExpenseSplitResponse> Splits
);

public record ExpenseSplitResponse(
    long    UserId,
    decimal OwedAmount,
    decimal PaidAmount,
    bool    IsSettled
);
