namespace FinanceService.DTO.Requests;

public record CreateCategoryRequest(
    string  Name,
    string  Kind,             // "INCOME" / "EXPENSE"
    string? Icon,
    string? Color
);
