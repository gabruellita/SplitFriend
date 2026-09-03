namespace IdentityService.DTO.Responses;

public record LoginResponse(
    string  AccessToken,
    string  RefreshToken,
    int     ExpiresIn,
    string  TokenType,
    UserDto User
);
