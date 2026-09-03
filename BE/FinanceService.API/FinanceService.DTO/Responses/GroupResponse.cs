namespace FinanceService.DTO.Responses;

public record GroupResponse(
    long     Id,
    string   Name,
    string?  Description,
    long     CurrencyId,
    string?  CurrencyCode,
    long     OwnerUserId,
    string   Status,
    long     MemberCount,
    string?  MyRole,
    DateTime CreatedAt
);
