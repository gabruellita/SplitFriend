namespace IdentityService.DTO.Responses;

public record RegisterResponse(
    long   UserId,
    string Email,
    string Username,
    string Status,
    string Message
);
