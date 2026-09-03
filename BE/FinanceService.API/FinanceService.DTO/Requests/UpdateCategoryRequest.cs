namespace FinanceService.DTO.Requests;

public record UpdateCategoryRequest(
    string  Name,
    string? Icon,
    string? Color
);
