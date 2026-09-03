using IdentityService.DTO.Requests;
using IdentityService.DTO.Responses;

namespace IdentityService.Services.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse>    LoginAsync(LoginRequest request);
    Task                   ConfirmEmailAsync(string token);
    Task<LoginResponse>    RefreshTokenAsync(string refreshToken);
    Task                   LogoutAsync(string refreshToken);
    Task                   ForgotPasswordAsync(string email, string frontendBaseUrl);
    Task                   ResetPasswordAsync(string token, string newPassword);
}
