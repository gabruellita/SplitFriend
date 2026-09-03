namespace IdentityService.DTO.Responses;

public record MeResponse(
    long    Id,
    string  Email,
    string  Username,
    string? FirstName,
    string? LastName,
    string  Status,
    long    PreferredCurrencyId,
    string? PreferredCurrencyCode,
    DateTime CreatedAt
);
