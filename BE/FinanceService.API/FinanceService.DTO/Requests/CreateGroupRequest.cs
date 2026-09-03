namespace FinanceService.DTO.Requests;

public record CreateGroupRequest(
    string  Name,
    string? Description,
    long    CurrencyId
);
