namespace FinanceService.DTO.Responses;

public record TransactionResponse(
    long     Id,
    decimal  Amount,
    string   Kind,
    DateOnly TransactionDate,
    long?    CategoryId,
    string?  CategoryName,
    long     CurrencyId,
    string?  CurrencyCode,
    string?  Description,
    string   Status,
    long?    TemplateId,
    DateTime CreatedAt
);
