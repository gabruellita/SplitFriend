namespace FinanceService.DTO.Responses;

public record SummaryResponse(
    decimal                        TotalIncome,
    decimal                        TotalExpense,
    decimal                        Net,
    IReadOnlyList<SummaryCategory> ByCategory
);

public record SummaryCategory(
    string  Kind,
    long?   CategoryId,
    string? CategoryName,
    decimal Total,
    long    Count
);
