namespace IdentityService.DTO.Requests;

public record ResetPasswordRequest(string Token, string NewPassword);
