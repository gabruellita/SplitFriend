namespace IdentityService.DTO.Requests;

public record RegisterRequest(
    string  Email,
    string  Username,
    string  Password,
    string? FirstName,
    string? LastName,
    long    PreferredCurrencyId
);
