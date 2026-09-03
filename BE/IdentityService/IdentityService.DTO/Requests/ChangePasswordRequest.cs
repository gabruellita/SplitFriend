namespace IdentityService.DTO.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
