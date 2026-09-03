namespace FinanceService.DTO.Responses;

public record CategoryResponse(
    long    Id,
    string  Name,
    string  Kind,
    string? Icon,
    string? Color,
    bool    IsSystem,
    bool    IsActive
);
