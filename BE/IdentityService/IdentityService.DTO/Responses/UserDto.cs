namespace IdentityService.DTO.Responses;

public record UserDto(
    long    Id,
    string  Email,
    string  Username,
    string? FirstName,
    string? LastName,
    string  Status,
    long    PreferredCurrencyId
);
