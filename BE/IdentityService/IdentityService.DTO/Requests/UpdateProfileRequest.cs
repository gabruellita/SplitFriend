namespace IdentityService.DTO.Requests;

public record UpdateProfileRequest(string? FirstName, string? LastName, long? PreferredCurrencyId);
