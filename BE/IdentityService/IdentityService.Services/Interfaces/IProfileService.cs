using IdentityService.DTO.Requests;
using IdentityService.DTO.Responses;

namespace IdentityService.Services.Interfaces;

public interface IProfileService
{
    Task<MeResponse> GetMeAsync(long userId);
    Task<MeResponse> UpdateProfileAsync(long userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(long userId, ChangePasswordRequest request);
}
